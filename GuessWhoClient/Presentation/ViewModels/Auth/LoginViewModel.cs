using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using log4net;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class LoginViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginViewModel));

        private const string LOG_CTX_LOGIN_RANGE = "LoginViewModel.Login.RangeError";
        private const string LOG_CTX_LOGIN_INVALID_OPERATION = "LoginViewModel.Login.InvalidOperation";
        private const string LOG_CTX_LOGIN_UNEXPECTED = "LoginViewModel.Login.Unexpected";

        private const string KEY_UI_ERROR_TITLE = "LoginErrorTitle";
        private const string KEY_UI_WELCOME_TITLE = "LoginWelcomeTitle";
        private const string KEY_UI_INVALID_CREDENTIALS = "LoginInvalidCredentials";

        private const string KEY_UI_RANGE_ERROR = "UiValueOutOfRange";
        private const string KEY_UI_INVALID_OPERATION = "UiInvalidOperation";
        private const string KEY_UI_UNEXPECTED_ERROR = "UiGenericError";

        private const string EMPTY = "";

        private readonly ILoginAppService loginAppService;
        private readonly IAlertService alertService;
        private readonly IUiFaultMapper loginFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;

        private string email;
        private string password;
        private bool isPasswordVisible;

        public LoginViewModel(
            ILoginAppService loginAppService,
            IAlertService alertService,
            IUiFaultMapper loginFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager)
        {
            this.loginAppService = loginAppService ?? 
                throw new ArgumentNullException(nameof(loginAppService));
            this.alertService = alertService ?? 
                throw new ArgumentNullException(nameof(alertService));
            this.loginFaultMapper = loginFaultMapper ?? 
                throw new ArgumentNullException(nameof(loginFaultMapper));
            this.localizationService = localizationService ?? 
                throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? 
                throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ?? 
                throw new ArgumentNullException(nameof(gameScreenManager));

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanExecuteCommands);
            CreateAccountCommand = new AsyncRelayCommand(OpenCreateAccountAsync, CanExecuteCommands);
            ForgotPasswordCommand = new AsyncRelayCommand(OpenRecoverPasswordAsync, CanExecuteCommands);
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

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoginCommand.RaiseCanExecuteChanged();
            CreateAccountCommand.RaiseCanExecuteChanged();
            ForgotPasswordCommand.RaiseCanExecuteChanged();
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
                if (HasEmptyCredentials())
                {
                    ShowInvalidCredentials();
                    return;
                }

                var request = new LoginRequest
                {
                    Email = Email,
                    Password = Password
                };

                var result = await loginAppService.LoginAsync(request);

                if (!result.IsSuccess)
                {
                    ShowLoginError(result.FaultCode, result.ServerMessage);
                    return;
                }

                if (result.Value == null || !result.Value.ValidUser)
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
                    localizationService.Get(KEY_UI_WELCOME_TITLE));

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

        private bool HasEmptyCredentials()
        {
            string safeEmail = (Email ?? EMPTY).Trim();
            string safePassword = (Password ?? EMPTY).Trim();

            return string.IsNullOrWhiteSpace(safeEmail) || string.IsNullOrWhiteSpace(safePassword);
        }

        private void ShowInvalidCredentials()
        {
            alertService.Error(
                localizationService.Get(KEY_UI_INVALID_CREDENTIALS),
                localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private void ShowUiError(string messageKey)
        {
            string message = localizationService.LocalOrFallback(
                messageKey,
                null,
                KEY_UI_UNEXPECTED_ERROR);

            alertService.Error(
                message,
                localizationService.Get(KEY_UI_ERROR_TITLE));
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

        private void ShowLoginError(string faultCode, string serverMessage)
        {
            string messageKey = ResolveUiKeyOrFallback(faultCode);

            string message = localizationService.LocalOrFallback(
                messageKey,
                serverMessage,
                KEY_UI_UNEXPECTED_ERROR);

            alertService.Error(
                message,
                localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private string ResolveUiKeyOrFallback(string faultCode)
        {
            UiKeyMapping mapping = loginFaultMapper.Map(faultCode);

            if (mapping.IsMapped)
            {
                return mapping.UiKey;
            }

            return KEY_UI_UNEXPECTED_ERROR;
        }
    }
}
