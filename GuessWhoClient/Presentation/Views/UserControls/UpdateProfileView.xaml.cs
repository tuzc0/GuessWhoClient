using System.Windows.Controls;
using GuessWhoClient.Presentation.ViewModels.Profile;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class UpdateProfileView : UserControl
    {
        public UpdateProfileView(UpdateProfileViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            Loaded += async (s, e) => await viewModel.LoadProfileAsync();
        }
    }
}