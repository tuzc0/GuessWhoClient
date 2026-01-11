using GuessWhoClient.Globalization;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using log4net;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Menu
{
    public sealed class MainMenuViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainMenuViewModel));

        private const string LOG_CTX_PLAY = "MainMenuViewModel.Play";
        private const string LOG_CTX_PROFILE = "MainMenuViewModel.Profile";
        private const string LOG_CTX_FRIENDS = "MainMenuViewModel.Friends"; 
        private const string LOG_CTX_LEADERBOARDS = "MainMenuViewModel.Leaderboards";
        private const string LOG_CTX_SETTINGS = "MainMenuViewModel.Settings";
        private const string LOG_CTX_EXIT = "MainMenuViewModel.Exit";

        private const string LOG_NAV_FAILED_TEMPLATE = "{0}: navigation failed. Code='{1}'. ScreenType='{2}'.";
        private const string LOG_NAV_EXCEPTION_TEMPLATE = "{0}: navigation threw exception for ScreenType='{1}'.";

        private const string KEY_UI_TITLE_WARNING = "UiTitleWarning";
        private const string KEY_UI_NAV_FACTORY_MISSING = "UiNavigationScreenFactoryMissing";
        private const string KEY_UI_NAV_FACTORY_RETURNED_NULL = "UiNavigationScreenFactoryReturnedNull";
        private const string KEY_UI_NAV_FAILED = "UiNavigationFailed";

        private readonly IGameScreenManager gameScreenManager;
        private readonly IAlertService alertService;
        private readonly ILocalizationService localizationService;

        public MainMenuViewModel(
            IGameScreenManager gameScreenManager,
            IAlertService alertService,
            ILocalizationService localizationService)
        {
            this.gameScreenManager = gameScreenManager ??
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.alertService = alertService ??
                throw new ArgumentNullException(nameof(alertService));
            this.localizationService = localizationService ??
                throw new ArgumentNullException(nameof(localizationService));

            PlayCommand = new RelayCommand(Play, CanExecuteCommands);
            ProfileCommand = new RelayCommand(Profile, CanExecuteCommands);
            FriendsCommand = new RelayCommand(Friends, CanExecuteCommands); 
            LeaderboardsCommand = new RelayCommand(Leaderboards, CanExecuteCommands);
            SettingsCommand = new RelayCommand(Settings, CanExecuteCommands);
            ExitCommand = new RelayCommand(Exit, CanExecuteCommands);
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand ProfileCommand { get; }
        public RelayCommand FriendsCommand { get; }
        public RelayCommand LeaderboardsCommand { get; }
        public RelayCommand SettingsCommand { get; }
        public RelayCommand ExitCommand { get; }

        private bool CanExecuteCommands() => !IsBusy;

        private void Play() => NavigateToScreen(GameScreenType.JoinOrCreateGame, LOG_CTX_PLAY);

        private void Profile() => NavigateToScreen(GameScreenType.UpdateProfile, LOG_CTX_PROFILE);

        private void Friends() => NavigateToScreen(GameScreenType.Friends, LOG_CTX_FRIENDS); 

        private void Leaderboards() => NavigateToScreen(GameScreenType.Leaderboard, LOG_CTX_LEADERBOARDS);

        private void Settings() => NavigateToOverlay(GameScreenType.Settings, LOG_CTX_SETTINGS);

        private void Exit()
        {
            try
            {
                System.Windows.Application.Current.Shutdown();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_EXIT, ex);
            }
        }

        private void NavigateToScreen(GameScreenType screenType, string context)
        {
            try
            {
                NavigationResult result = gameScreenManager.ShowScreen(screenType);
                if (!result.IsSuccess)
                {
                    HandleNavigationFailure(result, context);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Logger.Error(string.Format(LOG_NAV_EXCEPTION_TEMPLATE, context, screenType), ex);
                ShowNavigationWarning(KEY_UI_NAV_FAILED);
            }
        }

        private void NavigateToOverlay(GameScreenType screenType, string context)
        {
            try
            {
                NavigationResult result = gameScreenManager.ShowOverlay(screenType);
                if (!result.IsSuccess)
                {
                    HandleNavigationFailure(result, context);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Logger.Error(string.Format(LOG_NAV_EXCEPTION_TEMPLATE, context, screenType), ex);
                ShowNavigationWarning(KEY_UI_NAV_FAILED);
            }
        }

        private void HandleNavigationFailure(NavigationResult result, string context)
        {
            Logger.WarnFormat(LOG_NAV_FAILED_TEMPLATE, context, result.Code, result.ScreenType);
            string uiKey = MapNavigationCodeToUiKey(result.Code);
            ShowNavigationWarning(uiKey);
        }

        private string MapNavigationCodeToUiKey(string code)
        {
            if (string.Equals(code, NavigationCodes.CODE_SCREEN_FACTORY_MISSING, StringComparison.Ordinal))
            {
                return KEY_UI_NAV_FACTORY_MISSING;
            }
            if (string.Equals(code, NavigationCodes.CODE_SCREEN_FACTORY_RETURNED_NULL, StringComparison.Ordinal))
            {
                return KEY_UI_NAV_FACTORY_RETURNED_NULL;
            }
            return KEY_UI_NAV_FAILED;
        }

        private void ShowNavigationWarning(string messageKey)
        {
            alertService.Warn(
                localizationService.Get(messageKey),
                localizationService.Get(KEY_UI_TITLE_WARNING));
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            PlayCommand.RaiseCanExecuteChanged();
            ProfileCommand.RaiseCanExecuteChanged();
            FriendsCommand.RaiseCanExecuteChanged();
            LeaderboardsCommand.RaiseCanExecuteChanged();
            SettingsCommand.RaiseCanExecuteChanged();
            ExitCommand.RaiseCanExecuteChanged();
        }
    }
}