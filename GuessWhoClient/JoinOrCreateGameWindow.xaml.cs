using GuessWhoClient.Callbacks;
using GuessWhoClient.FriendServiceRef;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class JoinOrCreateGameWindow : UserControl
    {
        private readonly SessionContext sessionContext = SessionContext.Current;
        private MatchCallback matchCallBack;
        private MatchServiceClient matchClient;

        public JoinOrCreateGameWindow()
        {
            InitializeComponent();
            InitializeMatchClient();
        }

        private void InitializeMatchClient()
        {
            matchCallBack = new MatchCallback(Dispatcher);
            var context = new InstanceContext(matchCallBack);
            matchClient = new MatchServiceClient(context, "NetTcpBinding_IMatchService");
        }

        private async void BtnCreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this);

            try
            {
                var request = new CreateMatchRequest
                {
                    ProfileId = sessionContext.UserId
                };

                var response = await matchClient.CreateMatchAsync(request);
                await matchClient.SusbcribeLobbyAsync(response.MatchId);

                // --- CORRECCIÓN AQUÍ ---
                // Agregamos sessionContext.UserId al final
                var lobby = new GameLobbyWindow(response.MatchId, response.Code, response.Players, matchClient, sessionContext.UserId)
                {
                    Owner = ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                matchCallBack.AttachLobby(lobby);

                IsEnabled = false;

                lobby.Closed += (_, __) =>
                {
                    IsEnabled = true;
                    ownerWindow?.Activate();
                };

                lobby.Show();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al crear partida");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error inesperado");
            }
        }

        private async void BtnJoinExistingGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this);

            try
            {
                var code = txtCodeMatch.Text?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Ingresa un código de partida.", "Match");
                    return;
                }

                var request = new JoinMatchRequest
                {
                    UserId = sessionContext.UserId,
                    MatchCode = code
                };

                var response = await matchClient.JoinMatchAsync(request);
                await matchClient.SusbcribeLobbyAsync(response.MatchId);

                var lobby = new GameLobbyWindow(response.MatchId, response.Code, response.Players, matchClient, sessionContext.UserId)
                {
                    Owner = ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                matchCallBack.AttachLobby(lobby);

                IsEnabled = false;

                lobby.Closed += (_, __) =>
                {
                    IsEnabled = true;
                    ownerWindow?.Activate();
                };

                lobby.Show();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al unirse a partida");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error inesperado");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as Windows.GameWindow;
            ownerWindow?.LoadMainMenu();
        }
    }
}