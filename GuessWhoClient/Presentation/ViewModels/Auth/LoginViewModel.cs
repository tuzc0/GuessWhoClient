using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoClient.Windows.ScreensType;
using GuessWhoCore.Contracts.Requests;
using log4net;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class LoginViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginViewModel));

        private const string LOG_CTX_LOGIN = "LoginViewModel.Login";

        private const string KEY_UI_ERROR_TITLE = "LoginErrorTitle";
        private const string KEY_UI_WELCOME_TITLE = "LoginWelcomeTitle";
        private const string KEY_UI_INVALID_CREDENTIALS = "LoginInvalidCredentials";
        private const string KEY_UI_UNEXPECTED_ERROR = "UiGenericError";

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

            LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
            CreateAccountCommand = new AsyncRelayCommand(OpenCreateAccountAsync, () => !IsBusy);
            ForgotPasswordCommand = new AsyncRelayCommand(OpenRecoverPasswordAsync, () => !IsBusy);
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
        public ICommand CreateAccountCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            IsBusy = true;

            try
            {
                var request = new LoginRequest
                {
                    Email = Email,
                    Password = Password
                };

                var result = await loginAppService.LoginAsync(request);

                if (!result.IsSuccess)
                {
                    ShowLoginError(result.FaultCode, result.FaultCode);
                    return;
                }

                if (result.Value == null || !result.Value.ValidUser)
                {
                    alertService.Error(
                        localizationService.Get(KEY_UI_INVALID_CREDENTIALS),
                        localizationService.Get(KEY_UI_ERROR_TITLE));
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
                Logger.Warn(LOG_CTX_LOGIN, ex);
                ShowUnexpectedError();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_CTX_LOGIN, ex);
                ShowUnexpectedError();
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                ShowUnexpectedError();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowUnexpectedError()
        {
            alertService.Error(
                localizationService.Get(KEY_UI_UNEXPECTED_ERROR),
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

            alertService.Error(message, localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private string ResolveUiKeyOrFallback(string faultCode)
        {
            if (loginFaultMapper.TryMap(faultCode, out string uiKey))
            {
                return uiKey;
            }

            return KEY_UI_UNEXPECTED_ERROR;
        }
    }
}
