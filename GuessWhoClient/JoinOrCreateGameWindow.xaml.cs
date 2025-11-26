using GuessWhoClient.Callbacks;
using GuessWhoClient.Dtos;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.Windows;
using log4net;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class JoinOrCreateGameWindow : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JoinOrCreateGameWindow));

        private readonly SessionContext sessionContext = SessionContext.Current;

        public JoinOrCreateGameWindow()
        {
            InitializeComponent();
            Logger.Info("JoinOrCreateGameWindow initialized.");
        }

        private MatchServiceClient CreateMatchClient()
        {
            Logger.Info("Creating MatchServiceClient with MatchCallback for lobby operations.");

            var callback = new MatchCallback(Dispatcher);
            var context = new InstanceContext(callback);

            var client = new MatchServiceClient(context, "NetTcpBinding_IMatchService");

            Logger.Info("MatchServiceClient created successfully.");
            return client;
        }

        private async void BtnCreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;

            Logger.InfoFormat(
                "BtnCreateNewGame_Click invoked. SessionUserId={0}",
                sessionContext.UserId);

            try
            {
                var request = new CreateMatchRequest
                {
                    ProfileId = sessionContext.UserId
                };

                Logger.Info("Sending CreateMatchAsync request to MatchService.");

                using (var client = CreateMatchClient())
                {
                    var response = await client.CreateMatchAsync(request);

                    Logger.InfoFormat(
                        "CreateMatchAsync response received. MatchId={0}, Code={1}, PlayersCount={2}",
                        response.MatchId,
                        response.Code,
                        response.Players == null ? 0 : response.Players.Length);

                    var parametersMatchLobby = new GamePlayParameters(
                        response.MatchId,
                        response.Code,
                        response.Players);

                    Logger.Info("Navigating to GamePlayWindow (create game flow).");
                    ownerWindow?.CreateGamePlayWindow(parametersMatchLobby);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("FaultException when creating match.", ex);
                MessageBox.Show(ex.Detail.Message, "Error al crear partida");
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in BtnCreateNewGame_Click.", ex);
                MessageBox.Show(ex.Message, "Error inesperado");
            }
        }

        private async void BtnJoinExistingGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;

            Logger.Info("BtnJoinExistingGame_Click invoked.");

            try
            {
                var code = txtCodeMatch.Text?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    Logger.Warn("JoinExistingGame requested with empty match code.");
                    MessageBox.Show("Ingresa un código de partida.", "Match");
                    return;
                }

                Logger.InfoFormat(
                    "Attempting to join existing match. UserId={0}, Code={1}",
                    sessionContext.UserId,
                    code);

                var request = new JoinMatchRequest
                {
                    UserId = sessionContext.UserId,
                    MatchCode = code
                };

                using (var client = CreateMatchClient())
                {
                    var response = await client.JoinMatchAsync(request);

                    Logger.InfoFormat(
                        "JoinMatchAsync response received. MatchId={0}, Code={1}, PlayersCount={2}",
                        response.MatchId,
                        response.Code,
                        response.Players == null ? 0 : response.Players.Length);

                    var parametersMatchLobby = new GamePlayParameters(
                        response.MatchId,
                        response.Code,
                        response.Players);

                    Logger.Info("Navigating to GamePlayWindow (join game flow).");
                    ownerWindow?.CreateGamePlayWindow(parametersMatchLobby);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("FaultException when joining existing match.", ex);
                MessageBox.Show(ex.Detail.Message, "Error al unirse a partida");
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in BtnJoinExistingGame_Click.", ex);
                MessageBox.Show(ex.Message, "Error inesperado");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnBack_Click invoked. Returning to main menu.");

            var ownerWindow = Window.GetWindow(this) as GameWindow;
            ownerWindow?.LoadMainMenu();
        }
    }
}
