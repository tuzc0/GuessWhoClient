using GuessWhoClient.Callbacks;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using WPFGuessWhoClient;

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

            if (tbGameCode != null)
            {
                tbGameCode.Text = code;
            }

            foreach (LobbyPlayerDto player in players)
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
                // callback real que implementa IMatchServiceCallback
                matchCallback = new MatchCallback(Dispatcher);
                matchCallback.AttachLobby(this); // para reenviar a ILobbyClient (esta ventana)

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

        private async Task LeaveMatchAsync()
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

        private void BtnFriends_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Usa el usuario actual de sesión
                FriendsListWindow friendsWindow = new FriendsListWindow(CurrentUserId);
                friendsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open friends list window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
            LobbyPlayerDto existing = Players.FirstOrDefault(p => p.UserId == player.UserId);

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
            LobbyPlayerDto existing = Players.FirstOrDefault(p => p.UserId == player.UserId);
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
            // TODO: lógica cuando el servidor empiece la partida
        }

        public void OnGameLeft()
        {
            // TODO: lógica si el servidor avisa que se cerró la partida
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
                    // ignorar errores de unsubscribe
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
