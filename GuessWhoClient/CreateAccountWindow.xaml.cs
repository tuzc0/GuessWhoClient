using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Windows;
using GuessWhoClient.Alerts;
using GuessWhoClient.Dtos;
using GuessWhoClient.Globalization;
using GuessWhoClient.InputValidation;
using GuessWhoClient.UserServiceRef;
using log4net;

namespace GuessWhoClient
{
    public partial class CreateAccountWindow : Window
    {
        private const string USER_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IUserService";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreateAccountWindow));

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        public CreateAccountWindow()
        {
            InitializeComponent();
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            SetCreateAccountButtonEnabled(isEnabled: false);

            UserServiceClient client = null;

            try
            {
                RegisterRequest registerRequest = BuildRegisterRequestFromForm();
                client = new UserServiceClient(USER_SERVICE_ENDPOINT_NAME);

                RegisterResponse registerResponse = await client.RegisterUserAsync(registerRequest);

                alertService.Info(localizationService.Get("UiAccountCreatedForFmt"));

                if (registerResponse.EmailVerificationRequired)
                {
                    VerifyEmailWindow verifyWindow = new VerifyEmailWindow(
                        registerResponse.AccountId,
                        registerResponse.Email,
                        client);

                    bool? dialogResult = verifyWindow.ShowDialog();

                    if (dialogResult != true)
                    {
                        alertService.Warn(localizationService.Get("UiVerificationRequiredToLogin"));
                    }
                }

                await CloseUserServiceClientSafelyAsync(client);
                client = null;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn("Validation error while creating account.", ex);
                alertService.Warn(ex.Message);
            }
            catch (SecurityNegotiationException ex)
            {
                Logger.Error("Security negotiation failed while creating account.", ex);

                string message =
                    localizationService.Get("UiSecurityNegotiationFailed") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while creating account.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    "FaultUnexpected");

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while creating account.", ex);
                alertService.Error(localizationService.Get("FaultDatabaseTimeout"));
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while creating account.", ex);

                string message =
                    localizationService.Get("UiCommsGeneric") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error while creating account.", ex);

                string message =
                    localizationService.Get("FaultUnexpected") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
            }
            finally
            {
                if (client != null)
                {
                    await CloseUserServiceClientSafelyAsync(client);
                }

                SetCreateAccountButtonEnabled(isEnabled: true);
            }
        }

        private RegisterRequest BuildRegisterRequestFromForm()
        {
            AccountProfileInput newAccountProfile = new AccountProfileInput(
                txtEmail.Text,
                txtDisplayName.Text,
                txtPassword.Password,
                txtConfirmPassword.Password);

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

        private static async Task CloseUserServiceClientSafelyAsync(UserServiceClient client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    await Task.Run(() => client.Close());
                }
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
