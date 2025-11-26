using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Windows;
using System.Windows.Controls;
using GuessWhoClient.Alerts;
using GuessWhoClient.Dtos;
using GuessWhoClient.InputValidation;
using GuessWhoClient.LoginServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.Utilities;
using log4net;

namespace GuessWhoClient.Windows
{
    public partial class LoginWindow : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginWindow));

        private const string LOGIN_SERVICE_ENDPOINT_NAME = "NetTcpBinding_ILoginService";

        private const string LOGIN_SUCCESS_MESSAGE_FORMAT = "Welcome, {0}!";
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
        private readonly IAlertService alertService = new MessageBoxAlertService();

        public LoginWindow()
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

                    alertService.Info(string.Format(LOGIN_SUCCESS_MESSAGE_FORMAT, displayName));

                    LoadMainMenuWindow();
                }
                else
                {
                    Logger.Warn(LOG_LOGIN_INVALID_CREDENTIALS);
                    alertService.Error(LOGIN_INVALID_CREDENTIALS_MESSAGE);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_LOGIN_VALIDATION_ERROR, ex);
                alertService.Warn(ex.Message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_LOGIN_SERVICE_FAULT, ex);
                alertService.Error(ex.Detail.Message);
            }
            catch (MessageSecurityException ex)
            {
                Logger.Error(LOG_LOGIN_SECURITY_ERROR, ex);
                alertService.Error(LOGIN_SECURITY_ERROR_MESSAGE);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_LOGIN_ENDPOINT_NOT_FOUND, ex);
                alertService.Error(LOGIN_SERVICE_UNAVAILABLE_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_LOGIN_UNEXPECTED_ERROR, ex);
                alertService.Error(
                    LOGIN_UNEXPECTED_ERROR_PREFIX + ex.Message);
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
                User = loginInput.Email,
                Password = loginInput.Password
            };
        }

        private void SetLoginButtonEnabled(bool isEnabled)
        {
            btnLogin.IsEnabled = isEnabled;
        }

        private void ChkShowPasswords_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePasswordVisibilityFromCheckBoxState();
        }

        private void ChkShowPasswords_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePasswordVisibilityFromCheckBoxState();
        }

        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var visibilityContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdPassword,
                txtPasswordVisible);

            PasswordVisibilityHelper.SyncPasswordToText(visibilityContext);
        }

        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var visibilityContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdPassword,
                txtPasswordVisible);

            PasswordVisibilityHelper.SyncTextToPassword(visibilityContext);
        }

        private void UpdatePasswordVisibilityFromCheckBoxState()
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var visibilityContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdPassword,
                txtPasswordVisible);

            PasswordVisibilityHelper.TogglePasswordPair(visibilityContext);
        }

        private void CreateAccount_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void ForgotPassword_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ucRecoverPassword != null)
            {
                ucRecoverPassword.Visibility = Visibility.Visible;
            }
        }
    }
}