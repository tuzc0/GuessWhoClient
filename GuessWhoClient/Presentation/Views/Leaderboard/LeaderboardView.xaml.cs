using GuessWhoClient.Presentation.ViewModels.Leaderboard;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Leaderboard
{
    public partial class LeaderboardView : UserControl
    {
        public LeaderboardView(LeaderboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            this.Loaded += LeaderboardView_Loaded;
        }

        private void LeaderboardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LeaderboardViewModel viewModel)
            {
                if (viewModel.LoadLeaderboardCommand.CanExecute(null))
                {
                    viewModel.LoadLeaderboardCommand.Execute(null);
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}