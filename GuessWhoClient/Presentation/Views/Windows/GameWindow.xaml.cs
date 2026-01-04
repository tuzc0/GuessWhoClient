using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Views.Auth;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.UserServiceRef;
using GuessWhoClient.Windows.ScreensType;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Windows
{
    public sealed partial class GameWindow : Window, IGameScreenManager
    {
        private const string SCREEN_LOGIN = "Login";
        private const string SCREEN_CREATE_ACCOUNT = "CreateAccount";
        private const string SCREEN_MAIN_MENU = "MainMenu";
        private const string SCREEN_JOIN_OR_CREATE_GAME = "JoinOrCreateGame";
        private const string SCREEN_UPDATE_PROFILE = "UpdateProfile";
        private const string SCREEN_CHANGE_PASSWORD = "ChangePassword";

        private readonly Func<LoginView> loginViewFactory;
        private readonly Func<CreateAccountView> createAccountViewFactory;
        private readonly Func<MainMenuView> mainMenuViewFactory;
        private readonly Func<JoinOrCreateGameView> joinOrCreateGameViewFactory;
        private readonly Func<UpdateProfileView> updateProfileViewFactory;
        private readonly Func<ChangePasswordView> changePasswordViewFactory;

        public GameWindow(
            Func<LoginView> loginViewFactory,
            Func<CreateAccountView> createAccountViewFactory,
            Func<MainMenuView> mainMenuViewFactory,
            Func<JoinOrCreateGameView> joinOrCreateGameViewFactory,
            Func<UpdateProfileView> updateProfileViewFactory,
            Func<ChangePasswordView> changePasswordViewFactory)
        {
            this.loginViewFactory = loginViewFactory ?? 
                throw new ArgumentNullException(nameof(loginViewFactory));
            this.createAccountViewFactory = createAccountViewFactory ?? 
                throw new ArgumentNullException(nameof(createAccountViewFactory));
            this.mainMenuViewFactory = mainMenuViewFactory ?? 
                throw new ArgumentNullException(nameof(mainMenuViewFactory));
            this.joinOrCreateGameViewFactory = joinOrCreateGameViewFactory ?? 
                throw new ArgumentNullException(nameof(joinOrCreateGameViewFactory));
            this.updateProfileViewFactory = updateProfileViewFactory ?? 
                throw new ArgumentNullException(nameof(updateProfileViewFactory));
            this.changePasswordViewFactory = changePasswordViewFactory ?? 
                throw new ArgumentNullException(nameof(changePasswordViewFactory));

            InitializeComponent();
            LoadLoginWindow();
        }

        public void LoadLoginWindow() => ShowScreen(loginViewFactory());
        public void LoadCreateAccountWindow() => ShowScreen(createAccountViewFactory());
        public void LoadMainMenu() => ShowScreen(mainMenuViewFactory());
        public void LoadJoinOrCreateGameScreen() => ShowScreen(joinOrCreateGameViewFactory());
        public void LoadUpdateProfileScreen() => ShowScreen(updateProfileViewFactory());
        public void LoadChangePasswordScreen() => ShowScreen(changePasswordViewFactory());

        public void LoadVerifyEmailWindow(long accountId, string email, UserServiceClient client)
        {
            var verifyEmailScreen = new VerifyEmailView(accountId, email, client);
            ShowScreen(verifyEmailScreen);
        }

        public void ShowScreen(UserControl screen)
        {
            if (screen == null)
            {
                throw new ArgumentNullException(nameof(screen));
            }

            CloseOverlay();

            ScreenHost.Children.Clear();
            ScreenHost.Children.Add(screen);
        }

        public void ShowScreen(GameScreenType screenType)
        {
            string name = screenType.ToString();

            if (string.Equals(name, SCREEN_LOGIN, StringComparison.Ordinal))
            {
                LoadLoginWindow();
                return;
            }

            if (string.Equals(name, SCREEN_CREATE_ACCOUNT, StringComparison.Ordinal))
            {
                LoadCreateAccountWindow();
                return;
            }

            if (string.Equals(name, SCREEN_MAIN_MENU, StringComparison.Ordinal))
            {
                LoadMainMenu();
                return;
            }

            if (string.Equals(name, SCREEN_JOIN_OR_CREATE_GAME, StringComparison.Ordinal))
            {
                LoadJoinOrCreateGameScreen();
                return;
            }

            if (string.Equals(name, SCREEN_UPDATE_PROFILE, StringComparison.Ordinal))
            {
                LoadUpdateProfileScreen();
                return;
            }

            if (string.Equals(name, SCREEN_CHANGE_PASSWORD, StringComparison.Ordinal))
            {
                LoadChangePasswordScreen();
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(screenType), screenType, "Unknown screen type.");
        }

        public void ShowOverlay(UserControl overlay)
        {
            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            OverlayContent.Content = overlay;
            OverlayHost.Visibility = Visibility.Visible;
        }

        public void CloseOverlay()
        {
            OverlayContent.Content = null;
            OverlayHost.Visibility = Visibility.Collapsed;
        }

        public void ExitGame()
        {
            CloseOverlay();
            System.Windows.Application.Current.Shutdown();
        }

        public void CreateGamePlayWindow(GamePlayParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var gamePlayWindow = new GamePlayWindow(parameters)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            gamePlayWindow.Closed += (_, __) =>
            {
                Show();
                LoadJoinOrCreateGameScreen();
            };

            Hide();
            gamePlayWindow.Show();
        }
    }
}
