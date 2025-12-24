using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.UserServiceRef;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Windows
{
    public partial class GameWindow : Window
    {
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

        public void LoadLoginWindow()
        {
            ShowScreen(loginViewFactory());
        }

        public void LoadCreateAccountWindow()
        {
            ShowScreen(createAccountViewFactory());
        }

        public void LoadMainMenu()
        {
            ShowScreen(mainMenuViewFactory());
        }

        public void LoadJoinOrCreateGameScreen()
        {
            ShowScreen(joinOrCreateGameViewFactory());
        }

        public void LoadUpdateProfileScreen()
        {
            ShowScreen(updateProfileViewFactory());
        }

        public void LoadChangePasswordScreen()
        {
            ShowScreen(changePasswordViewFactory());
        }

        public void LoadVerifyEmailWindow(long accountId, string email, UserServiceClient client)
        {
            var verifyEmailScreen = new VerifyEmailView(accountId, email, client);
            ShowScreen(verifyEmailScreen);
        }

        public void ShowScreen(UserControl screen)
        {
            ScreenHost.Children.Clear();
            ScreenHost.Children.Add(screen);
        }

        public void CreateGamePlayWindow(GamePlayParameters parameters)
        {
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
