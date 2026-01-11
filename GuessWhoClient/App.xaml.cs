using GuessWhoClient.Application.ErrorHandling.Friends;
using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Application.Services.Leaderboard;
using GuessWhoClient.Application.Services.Profile;
using GuessWhoClient.Assets;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.ErrorHandling.Mapper;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Infraestructure.Session;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.Infraestructure.Wcf.Clients.Login;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.Services.Alerts;
using GuessWhoClient.Presentation.ViewModels.Auth;
using GuessWhoClient.Presentation.ViewModels.Match;
using GuessWhoClient.Presentation.ViewModels.Menu;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoClient.Presentation.ViewModels.Settings;
using GuessWhoClient.Presentation.ViewModels.Leaderboard;
using GuessWhoClient.Presentation.ViewModels.Friends;
using GuessWhoClient.Presentation.Views.Auth;
using GuessWhoClient.Presentation.Views.Menu;
using GuessWhoClient.Presentation.Views.Profile;
using GuessWhoClient.Presentation.Views.Settings;
using GuessWhoClient.Presentation.Views.Leaderboard;
using GuessWhoClient.Presentation.Views.Friends;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoClient.ViewModels.Profile;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;

namespace GuessWhoClient
{
    public partial class App : System.Windows.Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));
        private const string LOG_CTX_STARTUP_DI = "App.OnStartup.DI";
        private const string LOG_CTX_STARTUP = "App.OnStartup";
        private const string LOG_CTX_EXIT = "App.OnExit";
        private const string FATAL_STARTUP_TITLE = "Error";
        private const string FATAL_STARTUP_MESSAGE = "The application could not start due to a configuration error.";
        private IServiceProvider serviceProvider;

        public IServiceProvider Services => serviceProvider;
        public new static App Current => (App)System.Windows.Application.Current;

        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnStartup(e);
            try
            {
                var services = new ServiceCollection();
                RegisterCore(services);
                RegisterWcfClients(services);
                RegisterAppServices(services);
                RegisterNavigation(services);
                RegisterViewModels(services);
                RegisterViews(services);
                RegisterOverlayDialogs(services);
                RegisterMatchModule(services);
                serviceProvider = BuildServiceProviderValidated(services);
                var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
                MainWindow = mainWindow;
                mainWindow.Show();
                var screenManager = serviceProvider.GetRequiredService<IGameScreenManager>();
                screenManager.ShowScreen(GameScreenType.Login);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_STARTUP, ex);
                ShowFatalStartupError();
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try { (serviceProvider as IDisposable)?.Dispose(); }
            catch (Exception ex) { Logger.Error(LOG_CTX_EXIT, ex); }
            base.OnExit(e);
        }

        private static void RegisterCore(IServiceCollection services)
        {
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IGameMessageDialogService, GameMessageDialogService>();
            services.AddSingleton<IGameConfirmDialogService, GameConfirmDialogService>();
            services.AddSingleton<IAlertService, GameAlertService>();
            services.AddSingleton<Domain.Validation.IValidationIssueMapper, Domain.Validation.UserValidationIssueMapper>();
            services.AddSingleton<ICreateAccountValidationErrorsMapper, CreateAccountValidationErrorsMapper>();
            services.AddSingleton<SessionContext>(_ => SessionContext.Current);
            services.AddSingleton<AccountFlowContext>();
            services.AddSingleton<WcfCallExecutor>();
            services.AddSingleton<Dispatcher>(_ => Current?.Dispatcher ?? Dispatcher.CurrentDispatcher);
            services.AddSingleton<Func<string, string>>(sp => sp.GetRequiredService<ILocalizationService>().Get);
            services.AddSingleton<IAvatarPathResolver, AvatarPathResolver>();
            services.AddSingleton<UserRegistrationFaultUiCatalog>();
            services.AddSingleton<EmailVerificationFaultUiCatalog>();
            services.AddSingleton<PasswordRecoveryFaultUiCatalog>();
            services.AddSingleton<UpdateProfileFaultUiCatalog>();
            services.AddSingleton<InfrastructureEmailFaultUiCatalog>();
            services.AddSingleton<InfrastructureFaultUiCatalog>();
            services.AddSingleton<LeaderboardFaultUiCatalog>();
            services.AddSingleton<FriendFaultUiCatalog>();
            services.AddSingleton<IFaultUiCatalog, DefaultFaultUiCatalog>();
            services.AddSingleton<WcfUiFaultMapper>();
            services.AddSingleton<LoginUiFaultMapper>();
            services.AddSingleton<FriendUiFaultMapper>();
            services.AddSingleton<IUiFaultMapper>(sp =>
                new CompositeUiFaultMapper(
                    sp.GetRequiredService<LoginUiFaultMapper>(),
                    sp.GetRequiredService<UserRegistrationFaultUiCatalog>(),
                    sp.GetRequiredService<EmailVerificationFaultUiCatalog>(),
                    sp.GetRequiredService<PasswordRecoveryFaultUiCatalog>(),
                    sp.GetRequiredService<UpdateProfileFaultUiCatalog>(),
                    sp.GetRequiredService<InfrastructureEmailFaultUiCatalog>(),
                    sp.GetRequiredService<InfrastructureFaultUiCatalog>(),
                    sp.GetRequiredService<FriendFaultUiCatalog>(),
                    sp.GetRequiredService<LeaderboardFaultUiCatalog>(),
                    sp.GetRequiredService<WcfUiFaultMapper>()));

            services.AddSingleton<IPresenceHeartbeatService>(sp =>
            new PresenceHeartbeatService(
                sp.GetRequiredService<ILoginServiceClient>(),
                intervalSeconds: 20));

        }
        private static void RegisterWcfClients(IServiceCollection services)
        {
            services.AddSingleton<ILoginServiceClient, LoginServiceClientSessionAdapter>();

            services.AddTransient<IFriendServiceClient, FriendServiceClientAdapter>();
            services.AddTransient<ILeaderboardServiceClient, LeaderboardServiceClientAdapter>();
        }

        private static void RegisterAppServices(IServiceCollection services)
        {
            services.AddTransient<ILoginAppService, LoginAppService>();
            services.AddTransient<IUserAppService, UserAppService>();
            services.AddTransient<IFriendAppService, FriendAppService>();
            services.AddTransient<IUpdateProfileAppService, UpdateProfileAppService>();
            services.AddTransient<ILeaderboardAppService, LeaderboardAppService>();
        }

        private static void RegisterNavigation(IServiceCollection services)
        {
            services.AddSingleton<GameWindow>();
            services.AddSingleton<IGameScreenHost>(sp => sp.GetRequiredService<GameWindow>());
            services.AddSingleton<IReadOnlyDictionary<GameScreenType, Func<object>>>(sp =>
                new Dictionary<GameScreenType, Func<object>>
                {
                    { GameScreenType.Login, () => sp.GetRequiredService<LoginView>() },
                    { GameScreenType.CreateAccount, () => sp.GetRequiredService<CreateAccountView>() },
                    { GameScreenType.VerifyEmail, () => sp.GetRequiredService<VerifyEmailView>() },
                    { GameScreenType.RecoverPassword, () => sp.GetRequiredService<RecoverPasswordView>() },
                    { GameScreenType.MainMenu, () => sp.GetRequiredService<MainMenuView>() },
                    { GameScreenType.UpdateProfile, () => sp.GetRequiredService<UpdateProfileView>() },
                    { GameScreenType.ChangePassword, () => sp.GetRequiredService<ChangePasswordView>() },
                    { GameScreenType.JoinOrCreateGame, () => sp.GetRequiredService<JoinOrCreateGameView>() },
                    { GameScreenType.Settings, () => sp.GetRequiredService<SettingsView>() },
                    { GameScreenType.Leaderboard, () => sp.GetRequiredService<LeaderboardView>() },
                    { GameScreenType.Friends, () => sp.GetRequiredService<FriendView>() }
                });
            services.AddSingleton<IGameScreenManager, GameScreenManager>();
            services.AddSingleton<Func<LoginView>>(sp => () => sp.GetRequiredService<LoginView>());
            services.AddSingleton<Func<CreateAccountView>>(sp => () => sp.GetRequiredService<CreateAccountView>());
            services.AddSingleton<Func<MainMenuView>>(sp => () => sp.GetRequiredService<MainMenuView>());
            services.AddSingleton<Func<JoinOrCreateGameView>>(sp => () => sp.GetRequiredService<JoinOrCreateGameView>());
            services.AddSingleton<Func<UpdateProfileView>>(sp => () => sp.GetRequiredService<UpdateProfileView>());
            services.AddSingleton<Func<ChangePasswordView>>(sp => () => sp.GetRequiredService<ChangePasswordView>());
            services.AddSingleton<Func<SettingsView>>(sp => () => sp.GetRequiredService<SettingsView>());
            services.AddSingleton<Func<LeaderboardView>>(sp => () => sp.GetRequiredService<LeaderboardView>());
            services.AddSingleton<Func<FriendView>>(sp => () => sp.GetRequiredService<FriendView>());
        }

        private static void RegisterOverlayDialogs(IServiceCollection services)
        {
            services.AddSingleton<Func<ChooseAvatarView>>(sp => () => sp.GetRequiredService<ChooseAvatarView>());
            services.AddSingleton<Func<ChangePasswordView>>(sp => () => sp.GetRequiredService<ChangePasswordView>());
            services.AddSingleton<IOverlayDialogService, OverlayDialogService>();
        }

        private static void RegisterMatchModule(IServiceCollection services)
        {
            services.AddSingleton<MatchHub>(sp => new MatchHub(sp.GetRequiredService<Dispatcher>()));
            services.AddSingleton<IMatchClient>(sp => sp.GetRequiredService<MatchHub>());
            services.AddTransient<CreateOrJoinViewModel>(sp => {
                var session = sp.GetRequiredService<SessionContext>();
                return new CreateOrJoinViewModel(sp.GetRequiredService<MatchHub>(), sp.GetRequiredService<IAvatarPathResolver>(), session, session.UserId, session.UserId, sp.GetRequiredService<IUiFaultMapper>(), sp.GetRequiredService<Func<string, string>>());
            });
            services.AddTransient<JoinOrCreateGameView>(sp => {
                var view = new JoinOrCreateGameView();
                var viewModel = sp.GetRequiredService<CreateOrJoinViewModel>();
                view.DataContext = viewModel;
                var screenManager = sp.GetRequiredService<IGameScreenManager>();
                viewModel.BackRequested += () => screenManager.ShowScreen(GameScreenType.MainMenu);
                return view;
            });
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CreateAccountViewModel>();
            services.AddTransient<VerifyEmailViewModel>();
            services.AddTransient<RecoverPasswordViewModel>();
            services.AddTransient<MainMenuViewModel>();
            services.AddTransient<UpdateProfileViewModel>();
            services.AddTransient<ChooseAvatarViewModel>();
            services.AddTransient<ChangePasswordViewModel>();
            services.AddSingleton<Func<SettingsViewModel>>(sp => () => sp.GetRequiredService<SettingsViewModel>());
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<LeaderboardViewModel>();
            services.AddTransient<FriendViewModel>();
        }

        private static void RegisterViews(IServiceCollection services)
        {
            services.AddTransient<LoginView>(sp => CreateViewWithDataContext<LoginView, LoginViewModel>(sp));
            services.AddTransient<CreateAccountView>(sp => CreateViewWithDataContext<CreateAccountView, CreateAccountViewModel>(sp));
            services.AddTransient<VerifyEmailView>(sp => CreateViewWithDataContext<VerifyEmailView, VerifyEmailViewModel>(sp));
            services.AddTransient<RecoverPasswordView>(sp => CreateViewWithDataContext<RecoverPasswordView, RecoverPasswordViewModel>(sp));
            services.AddTransient<MainMenuView>(sp => CreateViewWithDataContext<MainMenuView, MainMenuViewModel>(sp));
            services.AddTransient<UpdateProfileView>(sp => CreateViewWithDataContext<UpdateProfileView, UpdateProfileViewModel>(sp));
            services.AddTransient<ChooseAvatarView>(sp => CreateViewWithDataContext<ChooseAvatarView, ChooseAvatarViewModel>(sp));
            services.AddTransient<ChangePasswordView>(sp => CreateViewWithDataContext<ChangePasswordView, ChangePasswordViewModel>(sp));
            services.AddTransient<LeaderboardView>(sp => CreateViewWithDataContext<LeaderboardView, LeaderboardViewModel>(sp));
            services.AddTransient<FriendView>(sp => CreateViewWithDataContext<FriendView, FriendViewModel>(sp));
            services.AddTransient<SettingsView>(sp => {
                var view = new SettingsView();
                var viewModel = sp.GetRequiredService<SettingsViewModel>();
                viewModel.RequestClose = () => sp.GetRequiredService<IGameScreenHost>().SetOverlayContent(null);
                view.DataContext = viewModel;
                return view;
            });
        }

        private static TView CreateViewWithDataContext<TView, TViewModel>(IServiceProvider serviceProvider) where TView : FrameworkElement
        {
            TView view = ActivatorUtilities.CreateInstance<TView>(serviceProvider);
            view.DataContext ??= serviceProvider.GetRequiredService<TViewModel>();
            return view;
        }

        private static IServiceProvider BuildServiceProviderValidated(ServiceCollection services)
        {
            return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        }

        private static void ShowFatalStartupError()
        {
            MessageBox.Show(FATAL_STARTUP_MESSAGE, FATAL_STARTUP_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}