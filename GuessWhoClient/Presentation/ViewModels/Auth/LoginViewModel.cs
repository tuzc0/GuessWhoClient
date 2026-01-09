using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling.Mapper;
using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class LoginViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginViewModel));

        private const string LOG_CTX_LOGIN_RANGE = "LoginViewModel.Login.RangeError";
        private const string LOG_CTX_LOGIN_INVALID_OPERATION = "LoginViewModel.Login.InvalidOperation";
        private const string LOG_CTX_LOGIN_UNEXPECTED = "LoginViewModel.Login.Unexpected";

        private const string KEY_UI_TITLE_ERROR = "UiLoginTitleError";
        private const string KEY_UI_TITLE_WELCOME = "UiLoginTitleWelcome";
        private const string KEY_UI_TITLE_WARNING = "UiTitleWarning";

        private const string KEY_UI_INVALID_CREDENTIALS = "UiLoginInvalidCredentials";

        private const string KEY_UI_RANGE_ERROR = "UiValueOutOfRange";
        private const string KEY_UI_INVALID_OPERATION = "UiInvalidOperation";
        private const string KEY_UI_UNEXPECTED_ERROR = "UiGenericError";

        private const string BULLET_PREFIX = "• ";
        private const string EMPTY = "";

        private readonly ILoginAppService loginAppService;
        private readonly IAlertService alertService;
        private readonly IFaultUiCatalog faultUiCatalog;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;
        private readonly IGameMessageDialogService gameMessageDialogService;

        private string email;
        private string password;
        private bool isPasswordVisible;

        public LoginViewModel(
            ILoginAppService loginAppService,
            IAlertService alertService,
            IFaultUiCatalog faultUiCatalog,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager,
            IGameMessageDialogService gameMessageDialogService)
        {
            this.loginAppService = loginAppService ??
                throw new ArgumentNullException(nameof(loginAppService));
            this.alertService = alertService ??
                throw new ArgumentNullException(nameof(alertService));
            this.faultUiCatalog = faultUiCatalog ??
                throw new ArgumentNullException(nameof(faultUiCatalog));
            this.localizationService = localizationService ??
                throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ??
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.gameMessageDialogService = gameMessageDialogService ??
                throw new ArgumentNullException(nameof(gameMessageDialogService));

            email = EMPTY;
            password = EMPTY;

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanExecuteCommands);
            CreateAccountCommand = new AsyncRelayCommand(OpenCreateAccountAsync, CanExecuteCommands);
            ForgotPasswordCommand = new AsyncRelayCommand(OpenRecoverPasswordAsync, CanExecuteCommands);
            OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync, CanExecuteCommands);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set => SetProperty(ref isPasswordVisible, value);
        }

        public AsyncRelayCommand LoginCommand { get; }
        public AsyncRelayCommand CreateAccountCommand { get; }
        public AsyncRelayCommand ForgotPasswordCommand { get; }
        public AsyncRelayCommand OpenSettingsCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoginCommand.RaiseCanExecuteChanged();
            CreateAccountCommand.RaiseCanExecuteChanged();
            ForgotPasswordCommand.RaiseCanExecuteChanged();
            OpenSettingsCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands()
        {
            return !IsBusy;
        }

        private async Task LoginAsync()
        {
            IsBusy = true;

            try
            {
                string validationMessage = BuildLoginValidationSummaryMessageOrEmpty();

                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    ShowValidationDialog(validationMessage);
                    return;
                }

                var request = new LoginRequest
                {
                    Email = (Email ?? EMPTY).Trim(),
                    Password = Password ?? EMPTY
                };

                var result = await loginAppService.LoginAsync(request);

                if (!result.IsSuccess)
                {
                    ShowLoginError(result.FaultCode, result.ServerMessage);
                    return;
                }

                if (!result.HasValue || result.Value == null || !result.Value.ValidUser)
                {
                    ShowInvalidCredentials();
                    return;
                }

                sessionContext.SignIn(
                    result.Value.UserId,
                    result.Value.DisplayName,
                    result.Value.Email,
                    result.Value.ValidUser);

                alertService.Info(
                    result.Value.DisplayName,
                    localizationService.Get(KEY_UI_TITLE_WELCOME_TITLE()));

                gameScreenManager.ShowScreen(GameScreenType.MainMenu);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logger.Warn(LOG_CTX_LOGIN_RANGE, ex);
                ShowUiError(KEY_UI_RANGE_ERROR);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_CTX_LOGIN_INVALID_OPERATION, ex);
                ShowUiError(KEY_UI_INVALID_OPERATION);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOGIN_UNEXPECTED, ex);
                ShowUiError(KEY_UI_UNEXPECTED_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string KEY_UI_TITLE_WELCOME_TITLE()
        {
            return KEY_UI_TITLE_WELCOME;
        }

        private string BuildLoginValidationSummaryMessageOrEmpty()
        {
            string safeEmail = (Email ?? EMPTY).Trim();
            string safePassword = Password ?? EMPTY;

            var draft = new LoginDraft(safeEmail, safePassword);

            IReadOnlyList<ValidationError> errors =
                UserRules.ValidateLogin(draft) ?? Array.Empty<ValidationError>();

            if (errors.Count == 0)
            {
                return EMPTY;
            }

            var uniqueMessages = new HashSet<string>(StringComparer.Ordinal);
            var messageBuilder = new StringBuilder();

            bool anyMessageAdded = false;

            for (int index = 0; index < errors.Count; index++)
            {
                string code = errors[index]?.Key ?? EMPTY;

                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                string uiKey = ResolveUiKeyOrFallback(code);

                if (string.IsNullOrWhiteSpace(uiKey) ||
                    string.Equals(uiKey, KEY_UI_UNEXPECTED_ERROR, StringComparison.Ordinal))
                {
                    continue;
                }

                string message = localizationService.LocalOrFallback(
                    uiKey, null, KEY_UI_UNEXPECTED_ERROR) ?? EMPTY;

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                message = message.Trim();

                if (!uniqueMessages.Add(message))
                {
                    continue;
                }

                messageBuilder.Append(BULLET_PREFIX);
                messageBuilder.AppendLine(message);

                anyMessageAdded = true;
            }

            return anyMessageAdded
                ? messageBuilder.ToString().Trim()
                : EMPTY;
        }

        private void ShowValidationDialog(string message)
        {
            string title =
                localizationService.Get(KEY_UI_TITLE_WARNING) ??
                localizationService.Get(KEY_UI_TITLE_ERROR) ??
                EMPTY;

            gameMessageDialogService.Show(title, message ?? EMPTY);
        }

        private void ShowInvalidCredentials()
        {
            alertService.Error(
                localizationService.Get(KEY_UI_INVALID_CREDENTIALS),
                localizationService.Get(KEY_UI_TITLE_ERROR));
        }

        private void ShowUiError(string messageKey)
        {
            string message = localizationService.LocalOrFallback(
                messageKey,
                null,
                KEY_UI_UNEXPECTED_ERROR);

            alertService.Error(
                message,
                localizationService.Get(KEY_UI_TITLE_ERROR));
        }

        private Task OpenCreateAccountAsync()
        {
            gameScreenManager.ShowScreen(GameScreenType.CreateAccount);
            return Task.CompletedTask;
        }

        private Task OpenRecoverPasswordAsync()
        {
            gameScreenManager.ShowOverlay(GameScreenType.RecoverPassword);
            return Task.CompletedTask;
        }

        private Task OpenSettingsAsync()
        {
            gameScreenManager.ShowOverlay(GameScreenType.Settings);
            return Task.CompletedTask;
        }

        private void ShowLoginError(string faultCode, string serverMessage)
        {
            string uiKey = ResolveUiKeyOrFallback(faultCode);

            string message = localizationService.LocalOrFallback(
                uiKey,
                serverMessage,
                KEY_UI_UNEXPECTED_ERROR);

            alertService.Error(
                message,
                localizationService.Get(KEY_UI_TITLE_ERROR));
        }

        private string ResolveUiKeyOrFallback(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return KEY_UI_UNEXPECTED_ERROR;
            }

            string uiKey = faultUiCatalog.ResolveUiKey(faultCode);

            return !string.IsNullOrWhiteSpace(uiKey)
                ? uiKey
                : KEY_UI_UNEXPECTED_ERROR;
        }
    }
}
