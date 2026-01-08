using GuessWhoClient.Application.Services.Leaderboard;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Session;
using GuessWhoContracts.Dtos.Dto;
using GuessWhoCore.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Leaderboard
{
    public partial class LeaderboardView : UserControl, INotifyPropertyChanged
    {
        private const string ERROR_TITLE = "Leaderboard Error";
        private const string ERROR_MESSAGE = "Could not load leaderboard data.";
        private const int TOP_N = 10;

        public ObservableCollection<LeaderboardPlayerDto> LeaderboardEntries { get; } =
            new ObservableCollection<LeaderboardPlayerDto>();

        private LeaderboardPlayerDto currentUserStats;
        private bool isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public LeaderboardPlayerDto CurrentUserStats
        {
            get => currentUserStats;
            set
            {
                currentUserStats = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public LeaderboardView()
        {
            InitializeComponent();
            DataContext = this;
            _ = LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                var app = (App)System.Windows.Application.Current;
                var leaderboardService = app.GetServiceProvider().GetRequiredService<ILeaderboardAppService>();
                var session = app.GetServiceProvider().GetRequiredService<SessionContext>();

                var request = new GetLeaderboardRequest
                {
                    TopN = TOP_N,
                    RequestingUserId = session.UserId
                };

                var result = await leaderboardService.GetGlobalLeaderboardAsync(request);

                if (result.IsSuccess && result.Value != null)
                {
                    LeaderboardEntries.Clear();
                    foreach (var player in result.Value.Players)
                    {
                        LeaderboardEntries.Add(player);
                    }

                    CurrentUserStats = result.Value.CurrentUserStats;
                }
                else
                {
                    MessageBox.Show(ERROR_MESSAGE, ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(ERROR_MESSAGE, ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadLeaderboardAsync();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            var screenManager = app.GetServiceProvider().GetRequiredService<IGameScreenManager>();
            screenManager.ShowScreen(GameScreenType.MainMenu);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}