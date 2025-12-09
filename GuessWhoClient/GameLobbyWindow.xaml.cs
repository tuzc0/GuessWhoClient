using GuessWhoClient.Callbacks;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.UserServiceRef;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using WPFGuessWhoClient;
using GuessWhoClient.Globalization; // Añadido para LocalizationProvider

namespace GuessWhoClient
{
    public partial class GameLobbyWindow : Window, ILobbyClient
    {
        private MatchServiceClient matchServiceClient;
        private MatchCallback matchCallback;
        private readonly long matchId; // matchId es long

        public long CurrentUserId => SessionContext.Current.UserId; // UserId es long

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
                matchCallback = new MatchCallback(Dispatcher);
                matchCallback.AttachLobby(this);

                var context = new InstanceContext(matchCallback);
                matchServiceClient = new MatchServiceClient(context, "NetTcpBinding_IMatchService");

                await matchServiceClient.SusbcribeLobbyAsync(matchId);
            }
            catch (Exception ex)
            {
                string errorTitle = LocalizationProvider.Instance["UiTitleError"];
                string errorMessage = LocalizationProvider.Instance["LobbyConnectionFailed"] + "\n" + ex.Message;

                MessageBox.Show(
                    errorMessage,
                    errorTitle,
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
                    MatchId = (int)matchId, // CORRECCIÓN 1: long a int
                    UserId = (int)CurrentUserId // CORRECCIÓN 2: long a int
                };

                BasicResponse response = await matchServiceClient.LeaveMatchAsync(request);

                if (!response.Success)
                {
                    string warningTitle = LocalizationProvider.Instance["UiTitleWarning"];
                    string warningMessage = LocalizationProvider.Instance["LobbyLeaveFailed"];

                    MessageBox.Show(
                        warningMessage,
                        warningTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            // Uso de ServiceFault de UserServiceRef y corrección de internacionalización
            catch (FaultException<UserServiceRef.ServiceFault> fault)
            {
                string errorTitle = LocalizationProvider.Instance["UiTitleError"];
                string faultMessage = LocalizationProvider.Instance[fault.Detail.Message];

                MessageBox.Show(
                    faultMessage,
                    errorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                string errorTitle = LocalizationProvider.Instance["UiTitleError"];
                string errorMessage = LocalizationProvider.Instance["LobbyUnexpectedLeaveError"];

                MessageBox.Show(
                    errorMessage,
                    errorTitle,
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
                // CORRECCIÓN 3: long a int para FriendsListWindow
                FriendsListWindow friendsWindow = new FriendsListWindow((int)CurrentUserId);
                friendsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorTitle = LocalizationProvider.Instance["UiTitleError"];
                string errorMessage = LocalizationProvider.Instance["FriendsListOpenError"] + $" {ex.Message}";

                MessageBox.Show(
                    errorMessage,
                    errorTitle,
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