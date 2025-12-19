using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Session;
using GuessWhoClient.Session.Controllers;
using GuessWhoClient.ViewModels;
using log4net;

namespace GuessWhoClient.Windows
{
    public partial class GamePlayWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayWindow));

        private readonly GamePlayParameters gamePlayParameters;
        private readonly IMatchSessionController matchSessionController;
        private readonly LobbyViewModel lobbyViewModel;

        public GamePlayWindow(GamePlayParameters gamePlayParameters)
        {
            InitializeComponent();

            this.gamePlayParameters = gamePlayParameters
                ?? throw new ArgumentNullException(nameof(gamePlayParameters));

            long currentUserId = SessionContext.Current.UserId;

            matchSessionController = new MatchSessionController(
                this.gamePlayParameters.MatchId,
                currentUserId);

            lobbyViewModel = new LobbyViewModel(this.gamePlayParameters, matchSessionController);

            matchSessionController.GameStarted += MatchSessionController_GameStarted;

            Logger.InfoFormat(
                "GamePlayWindow created. MatchId={0}, MatchCode={1}, PlayersCount={2}",
                this.gamePlayParameters.MatchId,
                this.gamePlayParameters.MatchCode,
                this.gamePlayParameters.Players == null
                    ? 0
                    : this.gamePlayParameters.Players.Count());

            Loaded += GamePlayWindow_Loaded;
            Closed += GamePlayWindow_Closed;
        }

        private async void GamePlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLobbyScreen();

            try
            {
                await matchSessionController.SubscribeLobbyAsync();
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while subscribing to lobby in GamePlayWindow_Loaded.", ex);

                MessageBox.Show(
                    "La suscripción al lobby excedió el tiempo de espera.",
                    "Error de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("Service fault while subscribing to lobby in GamePlayWindow_Loaded.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "El servidor devolvió un error al suscribirse al lobby.";

                MessageBox.Show(
                    message,
                    "Error del servidor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
            catch (FaultException ex)
            {
                Logger.Error("FaultException while subscribing to lobby in GamePlayWindow_Loaded.", ex);

                MessageBox.Show(
                    ex.Message,
                    "Error del servidor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while subscribing to lobby in GamePlayWindow_Loaded.", ex);

                MessageBox.Show(
                    "No fue posible comunicarse con el servidor del lobby.",
                    "Error de comunicación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error while subscribing to lobby in GamePlayWindow_Loaded.", ex);

                MessageBox.Show(
                    "Ocurrió un error inesperado al conectarse al lobby.",
                    "Error inesperado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private async void GamePlayWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                await matchSessionController.LeaveMatchAsync();
            }
            catch (TimeoutException ex)
            {
                Logger.Warn("Timeout while leaving match on GamePlayWindow close.", ex);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while leaving match on GamePlayWindow close.", ex);
            }
            catch (FaultException ex)
            {
                Logger.Warn("FaultException while leaving match on GamePlayWindow close.", ex);
            }
            catch (CommunicationException ex)
            {
                Logger.Warn("Communication error while leaving match on GamePlayWindow close.", ex);
            }
            catch (Exception ex)
            {
                Logger.Warn("Unexpected error while leaving match on GamePlayWindow close.", ex);
            }

            try
            {
                await matchSessionController.CloseAsync();
            }
            catch (TimeoutException ex)
            {
                Logger.Warn("Timeout while closing MatchSessionController on GamePlayWindow close.", ex);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while closing MatchSessionController on GamePlayWindow close.", ex);
            }
            catch (FaultException ex)
            {
                Logger.Warn("FaultException while closing MatchSessionController on GamePlayWindow close.", ex);
            }
            catch (CommunicationException ex)
            {
                Logger.Warn("Communication error while closing MatchSessionController on GamePlayWindow close.", ex);
            }
            catch (Exception ex)
            {
                Logger.Warn("Unexpected error while closing MatchSessionController on GamePlayWindow close.", ex);
            }

            matchSessionController.GameStarted -= MatchSessionController_GameStarted;
        }

        private void MatchSessionController_GameStarted()
        {
            Dispatcher.Invoke(LoadChooseCharacterScreen);
        }

        public void LoadLobbyScreen()
        {
            ScreenHost.Children.Clear();

            var lobbyScreen = new GameLobbyWindow(lobbyViewModel);

            ScreenHost.Children.Add(lobbyScreen);
        }

        public void LoadChooseCharacterScreen()
        {
            ScreenHost.Children.Clear();

            var chooseCharacterViewModel = new ChooseCharacterViewModel(matchSessionController);
            var chooseCharacterScreen = new ChooseCharacterWindow(chooseCharacterViewModel);
            matchSessionController.AllSecretCharactersChosen += matchId =>
            {
                // aquí marcas que ya no se puede cambiar
                chooseCharacterViewModel.IsSelectionLocked = true;
            };


            ScreenHost.Children.Add(chooseCharacterScreen);
        }
    }
}
