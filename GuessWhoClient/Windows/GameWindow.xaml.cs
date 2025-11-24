using GuessWhoClient.UserServiceRef;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Windows
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
            LoadLoginWindow();
        }

        public void LoadLoginWindow()
        {
            ScreenHost.Children.Clear();

            var loginScreen = new LoginWindow();
            
            ScreenHost.Children.Add(loginScreen);
        }

        public void LoadCreateAccountWindow()
        {
            ScreenHost.Children.Clear();
            
            var createAccountScreen = new CreateAccountWindow();
            
            ScreenHost.Children.Add(createAccountScreen);
        }

        public void LoadVerifyEmailWindow(long accountId, string email, UserServiceClient client)
        {
            ScreenHost.Children.Clear();

            var verifyEmailScreen = new VerifyEmailWindow(accountId, email, client);
            
            ScreenHost.Children.Add(verifyEmailScreen);
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

            var profileScreen = new ProfileWindow();
            
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
