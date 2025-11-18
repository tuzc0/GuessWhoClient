using GuessWhoClient.Globalization;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text.Trim();
            string password = pwdPassword.Password.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnLogin.IsEnabled = false;

            var client = new LoginServiceRef.LoginServiceClient("NetTcpBinding_ILoginService");


            try
            {
                var request = new LoginServiceRef.LoginRequest
                {
                    User = user,
                    Password = password
                };

                var response = await client.LoginUserAsync(request);

                if (response != null && response.ValidUser == "True")
                {
                    MessageBox.Show($"Welcome, {response.User}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    MainMenuWindow mainMenuWindow = new MainMenuWindow();
                    mainMenuWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid credentials.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                await Task.Run(() => client.Close());
            }
            catch (MessageSecurityException)
            {
                client.Abort();
                MessageBox.Show("Security error connecting to the service.", "Security Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (EndpointNotFoundException)
            {
                client.Abort();
                MessageBox.Show("Login service not available.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                client.Abort();
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item && item.Tag is string culture)
            {
                LocalizationProvider.Instance.ChangeCulture(culture);
            }
        }

        private void CreateAccount_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CreateAccountWindow createAccountWindow = new CreateAccountWindow();
            createAccountWindow.Owner = this;
            createAccountWindow.ShowDialog();
        }

        private void ForgotPassword_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
