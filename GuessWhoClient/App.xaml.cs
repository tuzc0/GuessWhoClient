using GuessWhoClient.Application.ErrorHandling.Friends;
using GuessWhoClient.Application.ErrorHandling.Profile;
using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Application.Services.Profile;
using GuessWhoClient.Assets;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.ErrorHandling.Mapper;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.Infraestructure.Wcf.Clients.Login;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.Services.Alerts;
using GuessWhoClient.Presentation.ViewModels.Auth;
using GuessWhoClient.Presentation.ViewModels.Match;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoClient.Presentation.ViewModels.Settings;
using GuessWhoClient.Presentation.Views.Auth;
using GuessWhoClient.Presentation.Views.Settings;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoClient.ViewModels.Profile;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GuessWhoClient
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            RegisterCore(services);
            RegisterWcfClients(services);
            RegisterAppServices(services);
            RegisterNavigation(services);
            RegisterViewModels(services);
            RegisterViews(services);
            RegisterMatchModule(services);

            serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            var screenManager = serviceProvider.GetRequiredService<IGameScreenManager>();
            screenManager.ShowScreen(GameScreenType.Login);
        }

        private static void RegisterCore(IServiceCollection services)
        {
            services.AddSingleton<ILocalizationService, LocalizationService>();

            services.AddSingleton<IGameMessageDialogService, GameMessageDialogService>();
            services.AddSingleton<IAlertService, GameAlertService>();

            services.AddSingleton<IValidationMessageMapper, ValidationMessageMapper>();

            services.AddSingleton<IValidationIssueMapper,
     GuessWhoClient.Presentation.ViewModels.Profile.CreateAccountValidationIssueMapper>();

            services.AddSingleton<ICreateAccountValidationErrorsMapper, CreateAccountValidationErrorsMapper>();

            services.AddSingleton<SessionContext>(_ => SessionContext.Current);
            services.AddSingleton<AccountFlowContext>();

            services.AddSingleton<WcfCallExecutor>();

            services.AddSingleton(sp => Current.Dispatcher);

            services.AddSingleton<Func<string, string>>(sp =>
                sp.GetRequiredService<ILocalizationService>().Get);

            services.AddSingleton<IAvatarPathResolver, AvatarPathResolver>();

            services.AddSingleton<IFaultUiCatalog, DefaultFaultUiCatalog>();

            services.AddSingleton<WcfUiFaultMapper>();
            services.AddSingleton<UserAccountUiFaultMapper>();
            services.AddSingleton<LoginUiFaultMapper>();
            services.AddSingleton<PasswordRecoveryUiFaultMapper>();
            services.AddSingleton<FriendUiFaultMapper>();
            services.AddSingleton<ProfileUiFaultMapper>();

            services.AddSingleton<IUiFaultMapper>(sp =>
                new CompositeUiFaultMapper(
                    sp.GetRequiredService<WcfUiFaultMapper>(),
                    sp.GetRequiredService<UserAccountUiFaultMapper>(),
                    sp.GetRequiredService<LoginUiFaultMapper>(),
                    sp.GetRequiredService<PasswordRecoveryUiFaultMapper>(),
                    sp.GetRequiredService<FriendUiFaultMapper>(),
                    sp.GetRequiredService<ProfileUiFaultMapper>()));
        }

        private static void RegisterWcfClients(IServiceCollection services)
        {
            services.AddTransient<ILoginServiceClient, LoginServiceClientAdapter>();
            services.AddTransient<IFriendServiceClient, FriendServiceClientAdapter>();
            services.AddTransient<IUpdateProfileServiceClient, UpdateProfileServiceClientAdapter>();
        }

        private static void RegisterAppServices(IServiceCollection services)
        {
            services.AddTransient<ILoginAppService, LoginAppService>();
            services.AddTransient<IUserAppService, UserAppService>();
            services.AddTransient<IFriendAppService, FriendAppService>();
            services.AddTransient<IUpdateProfileAppService, UpdateProfileAppService>();
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
                });

            services.AddSingleton<IGameScreenManager, GameScreenManager>();

            services.AddSingleton<Func<LoginView>>(sp => () => sp.GetRequiredService<LoginView>());
            services.AddSingleton<Func<CreateAccountView>>(sp => () => sp.GetRequiredService<CreateAccountView>());
            services.AddSingleton<Func<MainMenuView>>(sp => () => sp.GetRequiredService<MainMenuView>());
            services.AddSingleton<Func<JoinOrCreateGameView>>(sp => () => sp.GetRequiredService<JoinOrCreateGameView>());
            services.AddSingleton<Func<UpdateProfileView>>(sp => () => sp.GetRequiredService<UpdateProfileView>());
            services.AddSingleton<Func<ChangePasswordView>>(sp => () => sp.GetRequiredService<ChangePasswordView>());
            services.AddSingleton<Func<SettingsView>>(sp => () => sp.GetRequiredService<SettingsView>());
        }

        private static void RegisterMatchModule(IServiceCollection services)
        {
            services.AddSingleton<MatchHub>(sp => new MatchHub(sp.GetRequiredService<System.Windows.Threading.Dispatcher>()));

            services.AddSingleton<IMatchClient>(sp => sp.GetRequiredService<MatchHub>());

            services.AddTransient<CreateOrJoinViewModel>(sp =>
            {
                SessionContext session = sp.GetRequiredService<SessionContext>();
                long userId = session.UserId;

                return new CreateOrJoinViewModel(
                    sp.GetRequiredService<MatchHub>(),
                    sp.GetRequiredService<IAvatarPathResolver>(),
                    profileId: userId,
                    userId: userId,
                    sp.GetRequiredService<IUiFaultMapper>(),
                    sp.GetRequiredService<Func<string, string>>());
            });

            services.AddTransient<JoinOrCreateGameView>(sp =>
            {
                var view = new JoinOrCreateGameView();
                var vm = sp.GetRequiredService<CreateOrJoinViewModel>();

                view.DataContext = vm;

                IGameScreenManager screenManager = sp.GetRequiredService<IGameScreenManager>();

                void HandleBack()
                {
                    screenManager.ShowScreen(GameScreenType.MainMenu);
                }

                void HandleLobby(GameLobbyViewModel lobbyVm)
                {
                    var owner = Current.MainWindow;

                    var gamePlayWindow = new GamePlayWindow(lobbyVm)
                    {
                        Owner = owner,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    owner?.Hide();

                    gamePlayWindow.Closed += (_, __) =>
                    {
                        owner?.Show();
                        screenManager.ShowScreen(GameScreenType.JoinOrCreateGame);
                    };

                    gamePlayWindow.Show();
                }

                vm.BackRequested += HandleBack;
                vm.LobbyRequested += HandleLobby;

                view.Unloaded += (_, __) =>
                {
                    vm.BackRequested -= HandleBack;
                    vm.LobbyRequested -= HandleLobby;
                };

                return view;
            });
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CreateAccountViewModel>();
            services.AddTransient<VerifyEmailViewModel>();
            services.AddTransient<RecoverPasswordViewModel>();

            services.AddSingleton<Func<SettingsViewModel>>(sp =>
                () => sp.GetRequiredService<SettingsViewModel>());
            services.AddTransient<SettingsViewModel>();

        }


        private static void RegisterViews(IServiceCollection services)
        {
            services.AddTransient<LoginView>();
            services.AddTransient<CreateAccountView>();
            services.AddTransient<VerifyEmailView>();
            services.AddTransient<RecoverPasswordView>();

            services.AddTransient<MainMenuView>();
            services.AddTransient<JoinOrCreateGameView>();
            services.AddTransient<UpdateProfileView>();
            services.AddTransient<ChangePasswordView>();
            services.AddTransient<SettingsView>(sp =>
            {
                var view = new SettingsView();
                var vm = sp.GetRequiredService<SettingsViewModel>();

                var host = sp.GetRequiredService<IGameScreenHost>();
                vm.RequestClose = () => host.SetOverlayContent(null);

                view.DataContext = vm;
                return view;
            });

        }
    }
}
