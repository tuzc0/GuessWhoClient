using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.ViewModels;
using GuessWhoClient.Windows;
using log4net;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WPFGuessWhoClient;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class GameLobbyView : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameLobbyView));

        private readonly LobbyViewModel lobbyViewModel;

        public GameLobbyView(LobbyViewModel lobbyViewModel)
        {
            this.lobbyViewModel = lobbyViewModel
                ?? throw new ArgumentNullException(nameof(lobbyViewModel));

            InitializeComponent();

            DataContext = this.lobbyViewModel;

            Loaded += GameLobbyWindow_Loaded;
        }

        private void GameLobbyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.InfoFormat(
                "GameLobbyWindow loaded. MatchId={0}, Code={1}, PlayersCount={2}",
                lobbyViewModel.MatchId,
                lobbyViewModel.MatchCode,
                lobbyViewModel.LobbyPlayers.Count);
        }

        private async Task LeaveMatchAsync()
        {
            Logger.InfoFormat(
                "Leaving match from GameLobbyWindow. MatchId={0}, UserId={1}",
                lobbyViewModel.MatchId,
                lobbyViewModel.CurrentUserId);

            var result = await lobbyViewModel.LeaveAsync();

            if (!result.Success)
            {
                MessageBox.Show(
                    result.ErrorMessage ?? "Ocurrió un error al salir del lobby.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnBack_Click invoked. Leaving lobby and closing GamePlayWindow.");

            var ownerWindow = Window.GetWindow(this) as GamePlayWindow;

            btnBack.IsEnabled = false;

            try
            {
                await LeaveMatchAsync();
            }
            finally
            {
                btnBack.IsEnabled = true;
            }

            Logger.Info("Closing GamePlayWindow after leaving lobby.");
            ownerWindow?.Close();
        }

        private void BtnFriends_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("BtnFriends_Click invoked. Opening FriendsListWindow.");

            var friendsWindow = new FriendsListWindow(lobbyViewModel.CurrentUserId);
            friendsWindow.ShowDialog();

            Logger.Info("FriendsListWindow closed.");
        }

        private async Task SetCurrentPlayerReadyAsync()
        {
            Logger.InfoFormat(
                "Setting current player ready. MatchId={0}, UserId={1}",
                lobbyViewModel.MatchId,
                lobbyViewModel.CurrentUserId);

            var result = await lobbyViewModel.SetReadyAsync();

            if (!result.Success)
            {
                MessageBox.Show(
                    result.ErrorMessage ?? "Ocurrió un error al marcar tu estado como listo.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Lo usa el ToggleButton "LISTO" del invitado (abajo)
        // y cualquier ToggleButton de listo que tengas en la lista de jugadores.
        private async void ReadyToggle_Checked(object sender, RoutedEventArgs e)
        {
            var buttonBase = (ButtonBase)sender;

            buttonBase.IsEnabled = false;

            try
            {
                await SetCurrentPlayerReadyAsync();
            }
            finally
            {
                buttonBase.IsEnabled = true;
            }
        }

        private async Task StartMatchInternalAsync()
        {
            Logger.InfoFormat(
                "Requesting StartMatch from GameLobbyWindow. MatchId={0}, UserId={1}",
                lobbyViewModel.MatchId,
                lobbyViewModel.CurrentUserId);

            var result = await lobbyViewModel.StartMatchAsync();

            if (!result.Success)
            {
                MessageBox.Show(
                    result.ErrorMessage ?? "Ocurrió un error al iniciar la partida.",
                    "Error al iniciar partida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.IsEnabled = false;

            try
            {
                await StartMatchInternalAsync();
            }
            finally
            {
                button.IsEnabled = true;
            }
        }
    }
}
