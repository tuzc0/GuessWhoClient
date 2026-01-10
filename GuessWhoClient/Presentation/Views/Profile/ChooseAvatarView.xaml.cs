using GuessWhoClient.Presentation.ViewModels.Profile;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Profile
{
    public partial class ChooseAvatarView : UserControl
    {
        public ChooseAvatarView(ChooseAvatarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
