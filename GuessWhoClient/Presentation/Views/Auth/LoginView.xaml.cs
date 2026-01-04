using GuessWhoClient.Presentation.ViewModels.Auth;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Auth
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public LoginView(LoginViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
