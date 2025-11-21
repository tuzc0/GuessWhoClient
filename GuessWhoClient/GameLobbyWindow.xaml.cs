using GuessWhoClient.Callbacks;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
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
        private MatchServiceClient matchServiceClient;
        private MatchCallback matchCallback;
        private readonly long matchId;

        public long CurrentUserId => SessionContext.Current.UserId;

        public ObservableCollection<LobbyPlayerDto> Players { get; } =
            new ObservableCollection<LobbyPlayerDto>();

        public GameLobbyWindow(long matchId, string code, IEnumerable<LobbyPlayerDto> players)
        {
            InitializeComponent();

            this.matchId = matchId;
            tbGameCode.Text = code;

            foreach (var player in players)
            {
                Players.Add(player);
            }

            DataContext = this;

            Loaded += GameLobbyWindow_Loaded;
        }

        private async void GameLobbyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 🔹 callback real que implementa IMatchServiceCallback
                matchCallback = new MatchCallback(Dispatcher);
                matchCallback.AttachLobby(this); // para que reenvíe a ILobbyClient (esta ventana)

                var context = new InstanceContext(matchCallback);
                matchServiceClient = new MatchServiceClient(context, "NetTcpBinding_IMatchService");

                await matchServiceClient.SusbcribeLobbyAsync(matchId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible conectarse al lobby.\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private async System.Threading.Tasks.Task LeaveMatchAsync()
        {
            if (matchServiceClient == null)
            {
                return;
            }

            try
            {
                var request = new LeaveMatchRequest
                {
                    MatchId = matchId,
                    UserId = CurrentUserId
                };

                BasicResponse response = await matchServiceClient.LeaveMatchAsync(request);

                if (!response.Success)
                {
                    MessageBox.Show(
                        "No se pudo salir correctamente del lobby.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (FaultException<ServiceFault> fault)
            {
                MessageBox.Show(
                    fault.Detail.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Ocurrió un error inesperado al salir del lobby.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnBack_Click(object sender, RoutedEventArgs e)
        {
            btnBack.IsEnabled = false;

            await LeaveMatchAsync();

            Close();
        }

        #region ILobbyClient

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
            // TODO
        }

        public void OnGameLeft()
        {
            // TODO
        }

        #endregion

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (matchServiceClient == null)
            {
                return;
            }

            try
            {
                try
                {
                    await matchServiceClient.UnsusbcribeLobbyAsync(matchId);
                }
                catch
                {
                }

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
                matchServiceClient.Abort();
            }
        }
    }
}
