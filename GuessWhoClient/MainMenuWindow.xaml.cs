using GuessWhoClient.Session;
using GuessWhoClient.Windows;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class MainMenuScreen : UserControl
    {
        private readonly SessionContext sessionContext = SessionContext.Current;

        private const int NO_LOGGED_USER_ID = 0;

        public MainMenuScreen()
        {
            InitializeComponent();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                return;
            }

            if (sessionContext.UserId == NO_LOGGED_USER_ID)
            {
                gameWindow.LoadLoginWindow();
            }
            else
            {
                gameWindow.LoadUpdateProfileScreen();
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                return;
            }

            gameWindow.LoadJoinOrCreateGameScreen();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
