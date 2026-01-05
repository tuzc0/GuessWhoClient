using GuessWhoClient.Application.ErrorHandling.Auth;
using GuessWhoClient.Application.ErrorHandling.Friends;
using GuessWhoClient.Application.ErrorHandling.Profile;
using GuessWhoClient.Application.Services;
using GuessWhoClient.Application.Services.Auth;
using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Application.Services.Profile;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Friends;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using WPFGuessWhoClient.Presentation.ViewModels;

namespace GuessWhoClient
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IValidationMessageMapper, ValidationMessageMapper>();
            services.AddSingleton<IAlertService, MessageBoxAlertService>();
            services.AddSingleton<SessionContext>();
            services.AddSingleton<IGameScreenManager, GameScreenManager>();

            services.AddSingleton<LoginUiFaultMapper>();
            services.AddSingleton<FriendUiFaultMapper>();
            services.AddSingleton<UpdateProfileUiFaultMapper>();

            services.AddTransient<ILoginServiceClient, LoginServiceClientAdapter>();
            services.AddTransient<ILoginAppService, LoginAppService>();
            services.AddTransient<IUserAppService, UserAppService>();
            services.AddTransient<LoginView>();
            services.AddSingleton<Func<LoginView>>(provider => () => provider.GetRequiredService<LoginView>());

            services.AddTransient<IFriendServiceClient>(s =>
                new FriendServiceClientAdapter(s.GetRequiredService<FriendUiFaultMapper>()));
            services.AddTransient<IFriendAppService, FriendAppService>();
            services.AddTransient<FriendViewModel>();
            services.AddTransient<FriendManagerWindow>();
            services.AddSingleton<Func<FriendManagerWindow>>(provider => () => provider.GetRequiredService<FriendManagerWindow>());

            services.AddTransient<IUpdateProfileServiceClient>(s =>
                new UpdateProfileServiceClientAdapter(s.GetRequiredService<UpdateProfileUiFaultMapper>()));
            services.AddTransient<IUpdateProfileAppService, UpdateProfileAppService>();
            services.AddTransient<UpdateProfileViewModel>(s => new UpdateProfileViewModel(
                s.GetRequiredService<IUpdateProfileAppService>(),
                s.GetRequiredService<IAlertService>(),
                s.GetRequiredService<UpdateProfileUiFaultMapper>(),
                s.GetRequiredService<ILocalizationService>(),
                s.GetRequiredService<SessionContext>(),
                s.GetRequiredService<IGameScreenManager>()
            ));
            services.AddTransient<UpdateProfileView>();
            services.AddSingleton<Func<UpdateProfileView>>(provider => () => provider.GetRequiredService<UpdateProfileView>());

            services.AddTransient<CreateAccountViewModel>();
            services.AddTransient<CreateAccountView>();
            services.AddTransient<MainMenuView>();
            services.AddTransient<JoinOrCreateGameView>();
            services.AddTransient<ChangePasswordView>();

            services.AddSingleton<Func<CreateAccountView>>(provider => () => provider.GetRequiredService<CreateAccountView>());
            services.AddSingleton<Func<MainMenuView>>(provider => () => provider.GetRequiredService<MainMenuView>());
            services.AddSingleton<Func<JoinOrCreateGameView>>(provider => () => provider.GetRequiredService<JoinOrCreateGameView>());
            services.AddSingleton<Func<ChangePasswordView>>(provider => () => provider.GetRequiredService<ChangePasswordView>());

            services.AddSingleton<GameWindow>();

            serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
            mainWindow.Show();
        }
    }
}