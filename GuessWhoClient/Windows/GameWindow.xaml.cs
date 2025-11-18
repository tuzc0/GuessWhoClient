using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Windows
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
            LoadMainMenu();
        }

        public void LoadMainMenu()
        {
            ScreenHost.Children.Clear();

            var mainMenuScreen = new MainMenuScreen();

            ScreenHost.Children.Add(mainMenuScreen);
        }

        public void LoadJoinOrCreateGameScreen()
        {
            ScreenHost.Children.Clear();

            var joinOrCreateGameScreen = new JoinOrCreateGameWindow();
            
            ScreenHost.Children.Add(joinOrCreateGameScreen);
        }

        public void LoadUpdateProfileScreen()
        {
            ScreenHost.Children.Clear();

            var profileScreen = new Profile();
            
            ScreenHost.Children.Add(profileScreen);
        }

        public void LoadChangePasswordScreen()
        {
            ScreenHost.Children.Clear();

            var changePasswordScreen = new ChangePasswordDialog();
            ScreenHost.Children.Add(changePasswordScreen);
        }

        public void ShowScreen(UserControl screen)
        {
            ScreenHost.Children.Clear();
            ScreenHost.Children.Add(screen);
        }
    }
}
