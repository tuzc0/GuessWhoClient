using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace GuessWhoClient
{
    public partial class GameLobbyWindow : Window, ILobbyClient
    {
        private readonly MatchServiceClient matchServiceClient;
        private readonly long matchId;

        public ObservableCollection<LobbyPlayerDto> Players { get; } = new ObservableCollection<LobbyPlayerDto>();

        public GameLobbyWindow(long matchId, string code, IEnumerable<LobbyPlayerDto> players, MatchServiceClient matchClient)
        {
            InitializeComponent();

            this.matchServiceClient = matchClient;
            this.matchId = matchId;
            tbGameCode.Text = code;

            foreach (var player in players)
            {
                Players.Add(player);
            }

            DataContext = this; 
        }

        public void OnPlayerJoined(LobbyPlayerDto player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players.Add(player);
            });
        }

        public void OnPlayerLeft(LobbyPlayerDto player)
        {
            var existing = Players.FirstOrDefault(p => p.UserId == player.UserId);

            if (existing != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Players.Remove(existing);
                });
            }
        }

        public void OnReadyChanged(LobbyPlayerDto player)
        {
            var existing = Players.FirstOrDefault(p => p.UserId == player.UserId);
            if (existing != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    existing.IsReady = player.IsReady;
                });
            }
        }

        public void OnGameStarted()
        {
           
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            try
            {
                await matchServiceClient.UnsusbcribeLobbyAsync(matchId);

                if (matchServiceClient.State == CommunicationState.Faulted)
                {
                    matchServiceClient.Abort();
                }
                else
                {
                    matchServiceClient.Close();
                }
            }
            catch
            {
                matchServiceClient?.Abort();
            }
        }
    }
}
