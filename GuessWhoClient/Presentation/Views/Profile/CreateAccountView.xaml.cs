using GuessWhoClient.ViewModels.Profile;
using System;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class CreateAccountView
    {
        public CreateAccountView(CreateAccountViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
