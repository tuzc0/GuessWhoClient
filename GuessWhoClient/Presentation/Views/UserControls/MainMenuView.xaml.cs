using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Session;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.UserControls
{
    public partial class MainMenuView : UserControl
    {
        private readonly SessionContext sessionContext = SessionContext.Current;

        private const int NO_LOGGED_USER_ID = 0;

        public MainMenuView()
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
            System.Windows.Application.Current.Shutdown();
        }
    }
}
