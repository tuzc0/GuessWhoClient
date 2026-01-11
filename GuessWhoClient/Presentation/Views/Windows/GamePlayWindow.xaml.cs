using GuessWhoClient.Presentation.ViewModels.Match;
using GuessWhoClient.Presentation.Views.UserControls;
using log4net;
using System;
using System.ComponentModel;
using System.Windows;

namespace GuessWhoClient.Presentation.Views.Windows
{
    public partial class GamePlayWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayWindow));

        private readonly GameLobbyViewModel lobbyViewModel;
        private bool hasRequestedLeave;

        public GamePlayWindow(GameLobbyViewModel lobbyViewModel)
        {
            InitializeComponent();

            this.lobbyViewModel = lobbyViewModel
                ?? throw new ArgumentNullException(nameof(lobbyViewModel));

            this.lobbyViewModel.ExitRequested += LobbyViewModel_ExitRequested;

            ScreenHost.Content = new GameLobbyView
            {
                DataContext = this.lobbyViewModel
            };

            Closing += GamePlayWindow_Closing;
            Closed += GamePlayWindow_Closed;
        }

        private void LobbyViewModel_ExitRequested()
        {
            hasRequestedLeave = true;
            Dispatcher.Invoke(Close);
        }

        private void GamePlayWindow_Closing(object sender, CancelEventArgs e)
        {
            if (hasRequestedLeave)
            {
                return;
            }

            hasRequestedLeave = true;

            try
            {
                lobbyViewModel.LeaveLobbyCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Logger.Warn("GamePlayWindow.Closing: error executing LeaveLobbyCommand.", ex);
            }
        }

        private void GamePlayWindow_Closed(object sender, EventArgs e)
        {
            lobbyViewModel.ExitRequested -= LobbyViewModel_ExitRequested;
            lobbyViewModel.Dispose();
        }
    }
}
