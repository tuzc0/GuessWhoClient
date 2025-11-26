using GuessWhoClient.Callbacks;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.Windows;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFGuessWhoClient;

namespace GuessWhoClient
{
    public partial class GameLobbyWindow : UserControl, ILobbyClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameLobbyWindow));

        private const string MATCH_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IMatchService";

        private MatchServiceClient matchServiceClient;
        private MatchCallback matchCallback;
        private readonly long matchId;

        public long CurrentUserId
        {
            get { return SessionContext.Current.UserId; }
        }

        public ObservableCollection<LobbyPlayerDto> Players { get; } =
            new ObservableCollection<LobbyPlayerDto>();

        public bool IsCurrentUserHost
        {
            get
            {
                LobbyPlayerDto currentPlayer = Players.FirstOrDefault(p => p.UserId == CurrentUserId);

                if (currentPlayer == null)
                {
                    return false;
                }

                return currentPlayer.IsHost;
            }
        }

        public GameLobbyWindow(long matchId, string code, IEnumerable<LobbyPlayerDto> players)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error al inicializar GameLobbyWindow.", ex);
                MessageBox.Show(
                    "Error al inicializar la ventana de lobby:\n" + ex.Message,
                    "Error en GameLobbyWindow",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }

            this.matchId = matchId;

            if (tbGameCode != null)
            {
                tbGameCode.Text = code;
            }

            if (players != null)
            {
                foreach (LobbyPlayerDto player in players)
                {
                    Players.Add(player);
                }
            }

            DataContext = this;

            Loaded += GameLobbyWindow_Loaded;
        }


        private async void GameLobbyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                matchCallback = new MatchCallback(Dispatcher);
                matchCallback.AttachLobby(this);

                var context = new InstanceContext(matchCallback);
                matchServiceClient = new MatchServiceClient(context, MATCH_SERVICE_ENDPOINT_NAME);

                await matchServiceClient.SusbcribeLobbyAsync(matchId);

                Logger.Info("SubscribeLobbyAsync completed successfully.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("Service fault while initializing GameLobbyWindow and subscribing to lobby.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "Ocurrió un error en el servidor al conectarse al lobby.";

                MessageBox.Show(
                    message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (FaultException ex)
            {
                Logger.Error("FaultException while initializing GameLobbyWindow and subscribing to lobby.", ex);

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("CommunicationException while initializing GameLobbyWindow and subscribing to lobby.", ex);

                MessageBox.Show(
                    "No fue posible comunicarse con el servidor del lobby.",
                    "Error de comunicación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("TimeoutException while initializing GameLobbyWindow and subscribing to lobby.", ex);

                MessageBox.Show(
                    "La solicitud al servidor del lobby excedió el tiempo de espera.",
                    "Tiempo de espera agotado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LeaveMatchAsync()
        {
            if (matchServiceClient == null)
            {
                Logger.Warn("LeaveMatchAsync aborted because matchServiceClient is null.");
                return;
            }

            try
            {
                var request = new LeaveMatchRequest
                {
                    MatchId = matchId,
                    UserId = CurrentUserId
                };

                Logger.InfoFormat(
                    "Sending LeaveMatchAsync request. MatchId={0}, UserId={1}",
                    request.MatchId,
                    request.UserId);

                BasicResponse response = await matchServiceClient.LeaveMatchAsync(request);

                Logger.InfoFormat(
                    "LeaveMatchAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    MessageBox.Show(
                        "No se pudo salir correctamente del lobby.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("Service fault while leaving match.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "Ocurrió un error en el servidor al salir del lobby.";

                MessageBox.Show(
                    message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (FaultException ex)
            {
                Logger.Error("FaultException while leaving match.", ex);

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("CommunicationException while leaving match.", ex);

                MessageBox.Show(
                    "No fue posible comunicarse con el servidor para salir del lobby.",
                    "Error de comunicación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("TimeoutException while leaving match.", ex);

                MessageBox.Show(
                    "La solicitud para salir del lobby excedió el tiempo de espera.",
                    "Tiempo de espera agotado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnBack_Click invoked. Leaving lobby and closing GamePlayWindow.");

            var ownerWindow = Window.GetWindow(this) as GamePlayWindow;

            btnBack.IsEnabled = false;

            await LeaveMatchAsync();

            Logger.Info("Closing GamePlayWindow after leaving lobby.");
            ownerWindow?.Close();
        }

        private void BtnFriends_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnFriends_Click invoked. Opening FriendsListWindow.");

            var friendsWindow = new FriendsListWindow(CurrentUserId);
            friendsWindow.ShowDialog();

            Logger.Info("FriendsListWindow closed.");
        }

        public void OnPlayerJoined(LobbyPlayerDto player)
        {
            Logger.InfoFormat(
                "OnPlayerJoined callback received. UserId={0}, DisplayName={1}",
                player?.UserId ?? 0,
                player?.DisplayName ?? string.Empty);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Players.Add(player);
            });
        }

        public void OnPlayerLeft(LobbyPlayerDto player)
        {
            Logger.InfoFormat(
                "OnPlayerLeft callback received. UserId={0}",
                player?.UserId ?? 0);

            LobbyPlayerDto existing = Players.FirstOrDefault(p => p.UserId == player.UserId);

            if (existing != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Players.Remove(existing);
                });
            }
            else
            {
                Logger.Warn("OnPlayerLeft received but player not found in local collection.");
            }
        }

        public void OnReadyChanged(LobbyPlayerDto player)
        {
            Logger.InfoFormat(
                "OnReadyChanged callback received. UserId={0}, IsReady={1}",
                player?.UserId ?? 0,
                player?.IsReady ?? false);

            LobbyPlayerDto existing = Players.FirstOrDefault(p => p.UserId == player.UserId);

            if (existing != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    existing.IsReady = player.IsReady;
                });
            }
            else
            {
                Logger.Warn("OnReadyChanged received but player not found in local collection.");
            }
        }

        public void OnGameStarted()
        {
            Logger.Info("OnGameStarted callback received.");
            // TODO: lógica cuando el servidor empiece la partida
        }
    }
}
