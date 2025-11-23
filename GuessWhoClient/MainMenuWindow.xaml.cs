using GuessWhoClient.Session;
using GuessWhoClient.Windows;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class MainMenuScreen : UserControl
    {
        private readonly SessionContext sessionContext = SessionContext.Current;

        public MainMenuScreen()
        {
            InitializeComponent();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            Window ownerWindow = Window.GetWindow(this);

            if (sessionContext.UserId == 0)
            {
                var loginWindow = Window.GetWindow(this) as GameWindow;

                if (loginWindow == null)
                {
                    return;
                }

                loginWindow.LoadLoginWindow();
            }
            else
            {
                var profileWindow = Window.GetWindow(this) as GameWindow;

                if (profileWindow == null)
                {
                    return;
                }

                profileWindow.LoadUpdateProfileScreen();
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
