using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Windows;
using System.Windows.Controls;
using GuessWhoClient.Alerts;
using GuessWhoClient.Dtos;
using GuessWhoClient.Globalization;
using GuessWhoClient.InputValidation;
using GuessWhoClient.UserServiceRef;
using GuessWhoClient.Utilities;
using GuessWhoClient.Windows;
using log4net;

namespace GuessWhoClient
{
    public partial class CreateAccountWindow : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreateAccountWindow));

        private const string USER_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IUserService";
        private const string UI_ACCOUNT_CREATED_FOR_FMT_KEY = "UiAccountCreatedForFmt";
        private const string UI_SECURITY_NEGOTIATION_FAILED_KEY = "UiSecurityNegotiationFailed";
        private const string UI_COMMS_GENERIC_KEY = "UiCommsGeneric";
        private const string FAULT_UNEXPECTED_KEY = "FaultUnexpected";
        private const string FAULT_DATABASE_TIMEOUT_KEY = "FaultDatabaseTimeout";
        private const string VERIFY_EMAIL_HOST_NOT_FOUND_KEY = "UiVerifyEmailHostWindowNotFound";

        private const string VERIFY_EMAIL_HOST_NOT_FOUND_FALLBACK_MESSAGE =
            "No se encontró la ventana principal para continuar con la verificación de correo.";

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        public CreateAccountWindow()
        {
            InitializeComponent();
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            SetCreateAccountButtonEnabled(isEnabled: false);

            UserServiceClient userServiceClient = null;

            try
            {
                RegisterRequest registerRequest = BuildRegisterRequestFromForm();
                userServiceClient = new UserServiceClient(USER_SERVICE_ENDPOINT_NAME);

                RegisterResponse registerResponse = await userServiceClient.RegisterUserAsync(registerRequest);

                alertService.Info(localizationService.Get(UI_ACCOUNT_CREATED_FOR_FMT_KEY));

                if (registerResponse.EmailVerificationRequired)
                {
                    bool isVerifyEmailWindowLoaded = LoadVerifyEmailWindow(
                        registerResponse.AccountId,
                        registerResponse.Email,
                        userServiceClient);

                    if (isVerifyEmailWindowLoaded)
                    {
                        userServiceClient = null;
                        return;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn("Validation error while creating account.", ex);
                alertService.Warn(ex.Message);
            }
            catch (SecurityNegotiationException ex)
            {
                Logger.Error("Security negotiation failed while creating account.", ex);

                string message = BuildLocalizedErrorMessageWithException(
                    UI_SECURITY_NEGOTIATION_FAILED_KEY,
                    ex);

                alertService.Error(message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while creating account.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while creating account.", ex);
                alertService.Error(localizationService.Get(FAULT_DATABASE_TIMEOUT_KEY));
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while creating account.", ex);

                string message = BuildLocalizedErrorMessageWithException(
                    UI_COMMS_GENERIC_KEY,
                    ex);

                alertService.Error(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error while creating account.", ex);

                string message = BuildLocalizedErrorMessageWithException(
                    FAULT_UNEXPECTED_KEY,
                    ex);

                alertService.Error(message);
            }
            finally
            {
                if (userServiceClient != null)
                {
                    await ServiceClientGuard.CloseSafelyAsync(userServiceClient);
                }

                SetCreateAccountButtonEnabled(isEnabled: true);
            }
        }

        private RegisterRequest BuildRegisterRequestFromForm()
        {
            var newAccountProfile = new AccountProfileInput(
                txtEmail.Text,
                txtDisplayName.Text,
                pwdPassword.Password,
                pwdConfirmPassword.Password);

            List<string> errors = AccountValidator.ValidateForm(newAccountProfile);

            if (errors.Count > 0)
            {
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new InvalidOperationException(errorMessage);
            }

            return new RegisterRequest
            {
                Email = newAccountProfile.Email,
                Password = newAccountProfile.Password,
                DisplayName = newAccountProfile.DisplayName
            };
        }

        private void SetCreateAccountButtonEnabled(bool isEnabled)
        {
            btnCreateAccount.IsEnabled = isEnabled;
        }

        private void ChkShowPasswords_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePasswordsVisibilityFromCheckBoxState();
        }

        private void ChkShowPasswords_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePasswordsVisibilityFromCheckBoxState();
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

        private void PwdPasswordConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var visibilityContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdConfirmPassword,
                txtConfirmPasswordVisible);

            PasswordVisibilityHelper.SyncPasswordToText(visibilityContext);
        }

        private void TxtPasswordConfirmVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var visibilityContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdConfirmPassword,
                txtConfirmPasswordVisible);

            PasswordVisibilityHelper.SyncTextToPassword(visibilityContext);
        }

        private void UpdatePasswordsVisibilityFromCheckBoxState()
        {
            bool isPasswordVisible = chkShowPasswords.IsChecked == true;

            var mainPasswordContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdPassword,
                txtPasswordVisible);

            PasswordVisibilityHelper.TogglePasswordPair(mainPasswordContext);

            var confirmPasswordContext = new ChangePasswordVisibility(
                isPasswordVisible,
                pwdConfirmPassword,
                txtConfirmPasswordVisible);

            PasswordVisibilityHelper.TogglePasswordPair(confirmPasswordContext);
        }

        private bool LoadVerifyEmailWindow(long accountId, string email, UserServiceClient userServiceClient)
        {
            bool isVerifyEmailWindowLoaded = false;

            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                string message = localizationService.LocalOrFallback(
                    VERIFY_EMAIL_HOST_NOT_FOUND_KEY,
                    VERIFY_EMAIL_HOST_NOT_FOUND_FALLBACK_MESSAGE,
                    VERIFY_EMAIL_HOST_NOT_FOUND_KEY);

                alertService.Error(message);
                Logger.Error("GameWindow not found when trying to load VerifyEmailWindow.");
            }
            else
            {
                gameWindow.LoadVerifyEmailWindow(accountId, email, userServiceClient);
                isVerifyEmailWindowLoaded = true;
            }

            return isVerifyEmailWindowLoaded;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;
            gameWindow?.LoadLoginWindow();
        }

        private string BuildLocalizedErrorMessageWithException(string resourceKey, Exception ex)
        {
            string header = localizationService.Get(resourceKey);

            return header +
                   Environment.NewLine +
                   Environment.NewLine +
                   ex.Message;
        }
    }
}
