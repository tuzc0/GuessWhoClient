using GuessWhoClient.Presentation.ViewModels.Auth;
using System;

namespace GuessWhoClient.Presentation.Views.Auth
{
    public partial class VerifyEmailView
    {
        public VerifyEmailView(VerifyEmailViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
