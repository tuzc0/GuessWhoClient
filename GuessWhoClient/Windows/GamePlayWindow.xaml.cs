using System;
using System.Linq;
using System.Windows;
using GuessWhoClient.Dtos;
using log4net;

namespace GuessWhoClient.Windows
{
    public partial class GamePlayWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GamePlayWindow));

        private readonly GamePlayParameters gamePlayParameters;

        public GamePlayWindow(GamePlayParameters gamePlayParameters)
        {
            InitializeComponent();

            this.gamePlayParameters = gamePlayParameters ?? throw new ArgumentNullException(nameof(gamePlayParameters));

            Logger.InfoFormat(
                "GamePlayWindow created. MatchId={0}, MatchCode={1}, PlayersCount={2}",
                this.gamePlayParameters.MatchId,
                this.gamePlayParameters.MatchCode,
                this.gamePlayParameters.Players == null
                    ? 0
                    : this.gamePlayParameters.Players.Count());

            LoadLobbyScreen();
        }

        public void LoadLobbyScreen()
        {
            ScreenHost.Children.Clear();

            var lobbyScreen = new GameLobbyWindow(
                gamePlayParameters.MatchId,
                gamePlayParameters.MatchCode,
                gamePlayParameters.Players);

            ScreenHost.Children.Add(lobbyScreen);
        }
    }
}
