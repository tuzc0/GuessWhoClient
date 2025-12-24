using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GuessWhoClient.Alerts;
using GuessWhoClient.Dtos;
using GuessWhoClient.InputValidation;
using GuessWhoClient.LoginServiceRef;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Session;
using log4net;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class LoginView : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginView));

        private const string LOGIN_SERVICE_ENDPOINT_NAME = "NetTcpBinding_ILoginService";

        private const string LOGIN_ERROR_MESSAGE_FORMAT = "Error";
        private const string LOGIN_SUCCESS_MESSAGE_FORMAT = "Welcome";
        private const string LOGIN_INVALID_CREDENTIALS_MESSAGE = "Invalid credentials.";
        private const string LOGIN_SECURITY_ERROR_MESSAGE = "Security error connecting to the service.";
        private const string LOGIN_SERVICE_UNAVAILABLE_MESSAGE = "Login service not available.";
        private const string LOGIN_UNEXPECTED_ERROR_PREFIX = "Unexpected error: ";

        private const string LOG_LOGIN_INVALID_CREDENTIALS = "Login failed due to invalid credentials.";
        private const string LOG_LOGIN_VALIDATION_ERROR = "Validation error while building login request.";
        private const string LOG_LOGIN_SERVICE_FAULT = "Service fault received from LoginService.";
        private const string LOG_LOGIN_SECURITY_ERROR = "Security negotiation error while calling LoginService.";
        private const string LOG_LOGIN_ENDPOINT_NOT_FOUND = "LoginService endpoint not found.";
        private const string LOG_LOGIN_UNEXPECTED_ERROR = "Unexpected error during login process.";

        private const int NO_VALIDATION_ERRORS_COUNT = 0;

        private readonly SessionContext sessionContext = SessionContext.Current;

        public LoginView()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            SetLoginButtonEnabled(false);

            LoginServiceClient loginServiceClient = null;

            try
            {
                loginServiceClient = new LoginServiceClient(LOGIN_SERVICE_ENDPOINT_NAME);

                LoginRequest loginRequest = BuildLoginRequest();

                LoginResponse loginResponse = await loginServiceClient.LoginUserAsync(loginRequest);

                if (loginResponse != null && loginResponse.ValidUser)
                {
                    long userId = loginResponse.UserId;
                    string displayName = loginResponse.DisplayName;
                    string email = loginResponse.Email;
                    bool isValidUser = loginResponse.ValidUser;

                    sessionContext.SignIn(userId, displayName, email, isValidUser);

                    GameMessageBox.ShowInfo(LOGIN_SUCCESS_MESSAGE_FORMAT + displayName, LOGIN_SUCCESS_MESSAGE_FORMAT);

                    LoadMainMenuWindow();
                }
                else
                {
                    Logger.Warn(LOG_LOGIN_INVALID_CREDENTIALS);
                    GameMessageBox.ShowError(LOGIN_INVALID_CREDENTIALS_MESSAGE, LOGIN_INVALID_CREDENTIALS_MESSAGE);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_LOGIN_VALIDATION_ERROR, ex);
                GameMessageBox.ShowError(ex.Message, LOGIN_ERROR_MESSAGE_FORMAT);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_LOGIN_SERVICE_FAULT, ex);
                GameMessageBox.ShowError(ex.Detail.Message, LOGIN_ERROR_MESSAGE_FORMAT);
            }
            catch (MessageSecurityException ex)
            {
                Logger.Error(LOG_LOGIN_SECURITY_ERROR, ex);
                GameMessageBox.ShowError(LOGIN_SECURITY_ERROR_MESSAGE, LOGIN_ERROR_MESSAGE_FORMAT);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_LOGIN_ENDPOINT_NOT_FOUND, ex);
                GameMessageBox.ShowError(LOGIN_SERVICE_UNAVAILABLE_MESSAGE, LOGIN_ERROR_MESSAGE_FORMAT);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_LOGIN_UNEXPECTED_ERROR, ex);
                GameMessageBox.ShowError(
                    LOGIN_UNEXPECTED_ERROR_PREFIX + ex.Message, LOGIN_ERROR_MESSAGE_FORMAT);
            }
            finally
            {
                await ServiceClientGuard.CloseSafelyAsync(loginServiceClient);
                SetLoginButtonEnabled(true);
            }
        }

        private LoginRequest BuildLoginRequest()
        {
            string email = txtEmailUser.Text;
            string password = pwdPassword.Password;

            var loginInput = new LoginInput(
                email,
                password);

            List<string> errors = AccountValidator.ValidateLoginForm(loginInput);

            if (errors.Count > NO_VALIDATION_ERRORS_COUNT)
            {
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new InvalidOperationException(errorMessage);
            }

            return new LoginRequest
            {
                Email = loginInput.Email,
                Password = loginInput.Password
            };
        }

        private void SetLoginButtonEnabled(bool isEnabled)
        {
            btnLogin.IsEnabled = isEnabled;
        }

        private void CreateAccount_Click(object sender, MouseButtonEventArgs e)
        {
            var accountWindow = Window.GetWindow(this) as GameWindow;

            if (accountWindow == null)
            {
                return;
            }

            accountWindow.LoadCreateAccountWindow();
        }
        
        private void LoadMainMenuWindow()
        {
            var mainWindow = Window.GetWindow(this) as GameWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.LoadMainMenu();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (ucRecoverPassword != null)
            {
                ucRecoverPassword.Visibility = Visibility.Visible;
            }
        }
    }
}