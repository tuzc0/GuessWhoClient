using GuessWhoClient.Callbacks;
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

        public JoinOrCreateGameWindow()
        {
            InitializeComponent();
        }

        private MatchServiceClient CreateMatchClient()
        {
            var callback = new MatchCallback(Dispatcher);
            var context = new InstanceContext(callback);

            return new MatchServiceClient(context, "NetTcpBinding_IMatchService");
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

                using (var client = CreateMatchClient())
                {
                    var response = await client.CreateMatchAsync(request);

                    var lobby = new GameLobbyWindow(response.MatchId, response.Code, response.Players)
                    {
                        Owner = ownerWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    IsEnabled = false;

                    lobby.Closed += (_, __) =>
                    {
                        IsEnabled = true;
                        ownerWindow?.Activate();
                    };

                    lobby.Show();
                }
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

                using (var client = CreateMatchClient())
                {
                    var response = await client.JoinMatchAsync(request);

                    var lobby = new GameLobbyWindow(response.MatchId, response.Code, response.Players)
                    {
                        Owner = ownerWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    IsEnabled = false;

                    lobby.Closed += (_, __) =>
                    {
                        IsEnabled = true;
                        ownerWindow?.Activate();
                    };

                    lobby.Show();
                }
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
