using GuessWhoClient.Presentation.ViewModels.Menu;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Menu
{
    public partial class MainMenuView : UserControl
    {
        public MainMenuView(MainMenuViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
