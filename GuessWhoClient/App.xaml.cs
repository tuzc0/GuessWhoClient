using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Auth;
using GuessWhoClient.Presentation.ViewModels.Friends;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Services.Alerts;
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
            RegisterNavigation(services);
            RegisterAccountFlow(services);
            RegisterFriends(services);
            RegisterViews(services);
            RegisterFactories(services);

            serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static void RegisterCore(IServiceCollection services)
        {
            services.AddSingleton<ILocalizationService, LocalizationService>();

            services.AddSingleton<IAlertService, MessageBoxAlertService>();

            services.AddSingleton<WcfCallExecutor>();

            services.AddSingleton<IUiFaultMapper>(sp =>
                new CompositeUiFaultMapper(
                    new WcfUiFaultMapper(),
                    new UserAccountUiFaultMapper()));

            services.AddSingleton<AccountFlowContext>();
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
                    { GameScreenType.JoinOrCreateGame, () => sp.GetRequiredService<JoinOrCreateGameView>() },
                    { GameScreenType.UpdateProfile, () => sp.GetRequiredService<UpdateProfileView>() },
                    { GameScreenType.ChangePassword, () => sp.GetRequiredService<ChangePasswordView>() },

                    { GameScreenType.Lobby, () => sp.GetRequiredService<LobbyView>() },
                    { GameScreenType.Match, () => sp.GetRequiredService<MatchView>() },

                    { GameScreenType.Friends, () => sp.GetRequiredService<FriendView>() }
                });

            services.AddSingleton<IGameScreenManager, GameScreenManager>();
        }

        private static void RegisterAccountFlow(IServiceCollection services)
        {
            services.AddSingleton<IUserAppService, UserAppService>();

            services.AddSingleton<IValidationIssueMapper, Presentation.ViewModels.Profile.CreateAccountValidationIssueMapper>();
            services.AddSingleton<ICreateAccountValidationErrorsMapper, CreateAccountValidationErrorsMapper>();

            services.AddTransient<CreateAccountViewModel>();
            services.AddTransient<VerifyEmailViewModel>();
            services.AddTransient<RecoverPasswordViewModel>();
        }

        private static void RegisterFriends(IServiceCollection services)
        {
            services.AddTransient<IFriendAppService, FriendAppService>();
            services.AddTransient<FriendViewModel>();
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

            services.AddTransient<LobbyView>();
            services.AddTransient<MatchView>();

            services.AddTransient<FriendView>();
        }

        private static void RegisterFactories(IServiceCollection services)
        {
            services.AddSingleton<Func<LoginView>>(sp => () => sp.GetRequiredService<LoginView>());
            services.AddSingleton<Func<CreateAccountView>>(sp => () => sp.GetRequiredService<CreateAccountView>());
            services.AddSingleton<Func<VerifyEmailView>>(sp => () => sp.GetRequiredService<VerifyEmailView>());
            services.AddSingleton<Func<RecoverPasswordView>>(sp => () => sp.GetRequiredService<RecoverPasswordView>());

            services.AddSingleton<Func<MainMenuView>>(sp => () => sp.GetRequiredService<MainMenuView>());
            services.AddSingleton<Func<JoinOrCreateGameView>>(sp => () => sp.GetRequiredService<JoinOrCreateGameView>());
            services.AddSingleton<Func<UpdateProfileView>>(sp => () => sp.GetRequiredService<UpdateProfileView>());
            services.AddSingleton<Func<ChangePasswordView>>(sp => () => sp.GetRequiredService<ChangePasswordView>());

            services.AddSingleton<Func<FriendView>>(sp => () => sp.GetRequiredService<FriendView>());
        }
    }
}
