using GuessWhoClient.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class CreateAccountView : UserControl
    {
        public CreateAccountView(CreateAccountViewModel viewModel)
        {
            InitializeComponent();
            
            if (viewModel != null)
            {
                DataContext = viewModel ?? 
                    throw new ArgumentNullException(nameof(viewModel));
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateAccountViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateAccountViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
