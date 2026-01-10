using GuessWhoClient.Assets;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Match;
using GuessWhoClient.Presentation.ViewModels.Settings;
using GuessWhoClient.Presentation.Views.Auth;
using GuessWhoClient.Presentation.Views.Menu;
using GuessWhoClient.Presentation.Views.Profile;
using GuessWhoClient.Presentation.Views.Settings;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Session;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Windows
{
    public sealed partial class GameWindow : Window, IGameScreenHost
    {
        private const string MAIN_CONTENT_USERCONTROL_MESSAGE = "Main content must be a UserControl.";
        private const string OVERLAY_CONTENT_USERCONTROL_MESSAGE = "Overlay content must be a UserControl.";

        private readonly Func<LoginView> loginViewFactory;
        private readonly Func<SettingsView> settingsViewFactory;
        private readonly Func<SettingsViewModel> settingsViewModelFactory;
        private readonly Func<CreateAccountView> createAccountViewFactory;
        private readonly Func<MainMenuView> mainMenuViewFactory;
        private readonly Func<JoinOrCreateGameView> joinOrCreateGameViewFactory;
        private readonly Func<UpdateProfileView> updateProfileViewFactory;
        private readonly Func<ChangePasswordView> changePasswordViewFactory;

        private readonly MatchHub matchHub;
        private readonly IAvatarPathResolver avatarPathResolver;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly Func<string, string> localize;

        public GameWindow(
            Func<LoginView> loginViewFactory,
            Func<SettingsView> settingsViewFactory,
            Func<SettingsViewModel> settingsViewModelFactory,
            Func<CreateAccountView> createAccountViewFactory,
            Func<MainMenuView> mainMenuViewFactory,
            Func<JoinOrCreateGameView> joinOrCreateGameViewFactory,
            Func<UpdateProfileView> updateProfileViewFactory,
            Func<ChangePasswordView> changePasswordViewFactory,
            ILocalizationService localizationService)
        {
            this.loginViewFactory = loginViewFactory ??
                throw new ArgumentNullException(nameof(loginViewFactory));
            this.settingsViewFactory = settingsViewFactory ??
                throw new ArgumentNullException(nameof(settingsViewFactory));
            this.settingsViewModelFactory = settingsViewModelFactory ??
                throw new ArgumentNullException(nameof(settingsViewModelFactory));
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
            if (localizationService == null)
            {
                throw new ArgumentNullException(nameof(localizationService));
            }

            InitializeComponent();

            matchHub = new MatchHub(Dispatcher);
            avatarPathResolver = new AvatarPathResolver();
            uiFaultMapper = new CompositeUiFaultMapper(new WcfUiFaultMapper());
            localize = localizationService.Get;

            Loaded += (_, __) => LoadJoinOrCreateGameScreen();
        }

        public void LoadLoginWindow() => ShowScreen(loginViewFactory());
        public void LoadCreateAccountWindow() => ShowScreen(createAccountViewFactory());
        public void LoadMainMenu() => ShowScreen(mainMenuViewFactory());

        public void LoadSettingsWindow()
        {
            SettingsView view = settingsViewFactory();
            view.DataContext = settingsViewModelFactory();
            ShowOverlay(view);
        }

        public void LoadJoinOrCreateGameScreen()
        {
            JoinOrCreateGameView view = joinOrCreateGameViewFactory();

            long profileId = SessionContext.Current.UserId;
            long userId = SessionContext.Current.UserId;

            var vm = new CreateOrJoinViewModel(
                matchHub,
                avatarPathResolver,
                profileId,
                userId,
                uiFaultMapper,
                localize);

            vm.LobbyRequested += CreateGamePlayWindow;
            vm.BackRequested += LoadMainMenu;

            view.DataContext = vm;

            ShowScreen(view);
        }

        public void LoadUpdateProfileScreen() => ShowScreen(updateProfileViewFactory());
        public void LoadChangePasswordScreen() => ShowScreen(changePasswordViewFactory());

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

        public void ShowOverlay(UserControl overlay)
        {
            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            OverlayContent.Content = overlay;
            OverlayHost.Visibility = Visibility.Visible;
        }

        public void SetMainContent(object view)
        {
            if (Dispatcher.CheckAccess())
            {
                SetMainContentInternal(view);
                return;
            }

            Dispatcher.Invoke(() => SetMainContentInternal(view));
        }

        public void SetOverlayContent(object view)
        {
            if (Dispatcher.CheckAccess())
            {
                SetOverlayContentInternal(view);
                return;
            }

            Dispatcher.Invoke(() => SetOverlayContentInternal(view));
        }

        private void SetMainContentInternal(object view)
        {
            if (view is UserControl screen)
            {
                ShowScreen(screen);
                return;
            }

            throw new ArgumentException(MAIN_CONTENT_USERCONTROL_MESSAGE, nameof(view));
        }

        private void SetOverlayContentInternal(object view)
        {
            if (view == null)
            {
                CloseOverlay();
                return;
            }

            if (view is UserControl overlay)
            {
                ShowOverlay(overlay);
                return;
            }

            throw new ArgumentException(OVERLAY_CONTENT_USERCONTROL_MESSAGE, nameof(view));
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

        public void CreateGamePlayWindow(GameLobbyViewModel lobbyViewModel)
        {
            if (lobbyViewModel == null)
            {
                throw new ArgumentNullException(nameof(lobbyViewModel));
            }

            var gamePlayWindow = new GamePlayWindow(lobbyViewModel)
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
