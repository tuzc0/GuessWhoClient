using GuessWhoClient.UserServiceRef; 
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhoClient
{
    public partial class CreateAccountWindow : Window
    {
     
        private static readonly Regex EmailRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public CreateAccountWindow()
        {
            InitializeComponent();
        }

        private async void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            
            var email = (txtEmail.Text ?? string.Empty).Trim().ToLowerInvariant();
            var displayName = (txtDisplayName.Text ?? string.Empty).Trim();
            var password = txtPassword.Password ?? string.Empty;
            var confirm = txtConfirmPassword.Password ?? string.Empty;

            var validationError = ValidateForm(email, displayName, password, confirm);

            if (validationError != null)
            {
                ShowWarn(validationError);
                return;
            }

            btnCreateAccount.IsEnabled = false;

            var client = new UserServiceClient("NetTcp_UserService");

            try
            {
                var request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    DisplayName = displayName
                };

                var response = await client.RegisterUserAsync(request);
                var successMsg = string.Format(GetLocalizedText("UiAccountCreatedForFmt"));

                ShowInfo(successMsg);
                await SafeCloseAsync(client);
            }
            catch (SecurityNegotiationException ex)
            {
                client.Abort();
                ShowError(GetLocalizedText("UiSecurityNegotiationFailed") + "\n\n" + ex.Message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                client.Abort();
                string key = $"Fault.{ex.Detail.Code}";
                string text = LocalOrFallback(key, ex.Detail.Message, "FaultUnexpected");

                ShowWarn(text);
            }
            catch (TimeoutException)
            {
                client.Abort();
                ShowWarn(GetLocalizedText("FaultDatabaseTimeout"));
            }
            catch (CommunicationException ex)
            {
                client.Abort();
                ShowError(GetLocalizedText("UiCommsGeneric") + "\n\n" + ex.Message);
            }
            catch (Exception ex)
            {
                client.Abort();
                ShowError(GetLocalizedText("FaultUnexpected") + "\n\n" + ex.Message);
            }
            finally
            {
                btnCreateAccount.IsEnabled = true;
            }
        }

        private static void ShowWarn(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void ShowInfo(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleError"), MessageBoxButton.OK, MessageBoxImage.Error);

        private static string GetLocalizedText(string message)
        {
            return Globalization.LocalizationProvider.Instance[message];
        }

        private static string LocalOrFallback(string key, string serverMessage, string fallbackKey)
        {
            var text = GetLocalizedText(key);

            if (!string.IsNullOrWhiteSpace(serverMessage))
            {
                return serverMessage;
            }

            return GetLocalizedText(fallbackKey);                                   
        }

        private static string ValidateForm(string email, string displayName, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return GetLocalizedText("UiValidationEmailRequired");
            }

            if (!EmailRegex.IsMatch(email))
            {
                return GetLocalizedText("UiValidationEmailFormat");              
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                return GetLocalizedText("UiValidationDisplayNameRequired");      
            }

            if (displayName.Length > 50)
            {
                return GetLocalizedText("UiValidationDisplayNameTooLong");      
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return GetLocalizedText("UiValidationPasswordRequired");        
            }

            if (password.Length < 8)
            {
                return GetLocalizedText("UiValidationPasswordTooShort");        
            }

            if (!string.Equals(password, confirmPassword))
            {
                return GetLocalizedText("UiValidationPasswordsDontMatch");     
            }

            return null;
        }

        private static async Task SafeCloseAsync(UserServiceClient client)
        {
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
