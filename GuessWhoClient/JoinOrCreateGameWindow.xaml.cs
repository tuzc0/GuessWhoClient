using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using GuessWhoClient.Callbacks;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.Windows;
using log4net;
using ServiceLobbyPlayerDto = GuessWhoClient.MatchServiceRef.LobbyPlayerDto;
using ClientLobbyPlayerDto = GuessWhoClient.Dtos.ClientLobbyPlayerDto;


namespace GuessWhoClient
{
    public partial class JoinOrCreateGameWindow : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JoinOrCreateGameWindow));

        private const string MATCH_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IMatchService";

        private readonly SessionContext sessionContext = SessionContext.Current;

        private MatchCallback matchCallback;

        public JoinOrCreateGameWindow()
        {
            InitializeComponent();
        }

        private MatchServiceClient CreateMatchClient()
        {
            if (matchCallback == null)
            {
                matchCallback = new MatchCallback(Dispatcher);
            }

            var typedCallback = matchCallback as IMatchServiceCallback;

            var context = new InstanceContext(matchCallback);

            var client = new MatchServiceClient(context, MATCH_SERVICE_ENDPOINT_NAME);

            return client;
        }

        private async void BtnCreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;

            MatchServiceClient client = null;

            try
            {
                var request = new CreateMatchRequest
                {
                    ProfileId = sessionContext.UserId
                };

                client = CreateMatchClient();

                var response = await client.CreateMatchAsync(request);
                var players = MapLobbyPlayers(response.Players);

                var gameParams = new GamePlayParameters(
                    response.MatchId,
                    response.Code,
                    players);

                ownerWindow?.CreateGamePlayWindow(gameParams);

                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                client?.Abort();

                var message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "The server returned an error while creating the match.";

                MessageBox.Show(
                    message,
                    "Error creating match",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout calling CreateMatchAsync.", ex);
                client?.Abort();

                MessageBox.Show(
                    "The service took too long to respond when creating the match.",
                    "Timeout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error calling CreateMatchAsync.", ex);
                client?.Abort();

                MessageBox.Show(
                    "Communication error with the match service.",
                    "Communication error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in BtnCreateNewGame_Click.", ex);
                client?.Abort();

                MessageBox.Show(
                    ex.Message,
                    "Unexpected error while creating match",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnJoinExistingGame_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;

            Logger.Info("BtnJoinExistingGame_Click invoked.");

            MatchServiceClient client = null;

            try
            {
                var code = txtCodeMatch.Text?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    Logger.Warn("JoinExistingGame requested with empty match code.");
                    MessageBox.Show(
                        "Please enter a match code.",
                        "Match",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var request = new JoinMatchRequest
                {
                    UserId = sessionContext.UserId,
                    MatchCode = code
                };

                client = CreateMatchClient();

                var response = await client.JoinMatchAsync(request);

                var players = MapLobbyPlayers(response.Players);

                var gameParams = new GamePlayParameters(
                    response.MatchId,
                    response.Code,
                    players);

                Logger.Info("Navigating to GamePlayWindow (join game flow).");
                ownerWindow?.CreateGamePlayWindow(gameParams);

                if (client.State == CommunicationState.Faulted)
                {
                    Logger.Warn("MatchServiceClient in Faulted state after JoinMatchAsync. Aborting instead of closing.");
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("FaultException when joining existing match.", ex);
                client?.Abort();

                var message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "The server returned an error while joining the match.";

                MessageBox.Show(
                    message,
                    "Error joining match",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout when joining existing match.", ex);
                client?.Abort();

                MessageBox.Show(
                    "The service took too long to respond when joining the match.",
                    "Timeout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error when joining existing match.", ex);
                client?.Abort();

                MessageBox.Show(
                    "Communication error with the match service.",
                    "Communication error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in BtnJoinExistingGame_Click.", ex);
                client?.Abort();

                MessageBox.Show(
                    ex.Message,
                    "Unexpected error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static IReadOnlyList<ClientLobbyPlayerDto> MapLobbyPlayers(ServiceLobbyPlayerDto[] players)
        {
            if (players == null || players.Length == 0)
            {
                return Array.Empty<ClientLobbyPlayerDto>();
            }

            var result = new ClientLobbyPlayerDto[players.Length];

            for (int index = 0; index < players.Length; index++)
            {
                ServiceLobbyPlayerDto p = players[index];

                result[index] = new ClientLobbyPlayerDto
                {
                    MatchId = p.MatchId,
                    UserId = p.UserId,
                    DisplayName = p.DisplayName,
                    Avatar = p.AvatarId,
                    SlotNumber = p.SlotNumber,
                    IsReady = p.IsReady,
                    IsHost = p.IsHost
                };
            }

            return result;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnBack_Click invoked. Returning to main menu.");

            var ownerWindow = Window.GetWindow(this) as GameWindow;
            ownerWindow?.LoadMainMenu();
        }
    }
}
