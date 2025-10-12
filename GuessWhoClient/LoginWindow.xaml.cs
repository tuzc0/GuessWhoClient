using System;
using System.Windows;
using System.Windows.Controls;
using GuessWhoClient.Globalization;

namespace GuessWhoClient
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // falta implementar logica
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
            // falta implementar logica
        }
    }
}
