using GuessWhoClient.Presentation.ViewModels.Auth;
using System;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class RecoverPasswordView
    {
        public RecoverPasswordView(RecoverPasswordViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
