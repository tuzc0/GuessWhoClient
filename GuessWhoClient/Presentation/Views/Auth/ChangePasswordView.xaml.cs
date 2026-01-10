using GuessWhoClient.Presentation.ViewModels.Auth;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Auth
{
    public partial class ChangePasswordView : UserControl
    {
        public ChangePasswordView(ChangePasswordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
