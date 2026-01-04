using GuessWhoClient.Application.Services;
using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.Presentation.ViewModels.Friends;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.ViewModels;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using WPFGuessWhoClient;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IValidationMessageMapper, ValidationMessageMapper>();
            services.AddSingleton<IUserAppService, UserAppService>();
            services.AddSingleton<SessionContext>();
            services.AddSingleton<IAlertService, AlertService>();

            services.AddTransient<IFriendServiceClient, FriendServiceClientAdapter>();
            services.AddTransient<IFriendAppService, FriendAppService>();
            services.AddSingleton<FriendUiFaultMapper>();

            services.AddTransient<FriendViewModel>(provider =>
            {
                return new FriendViewModel(
                    provider.GetRequiredService<IFriendAppService>(),
                    provider.GetRequiredService<IAlertService>(),
                    provider.GetRequiredService<FriendUiFaultMapper>(),
                    provider.GetRequiredService<ILocalizationService>(),
                    provider.GetRequiredService<SessionContext>()
                );
            });

            services.AddTransient<FriendManagerWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<CreateAccountViewModel>();
            services.AddTransient<CreateAccountView>();
            services.AddTransient<MainMenuView>();
            services.AddTransient<JoinOrCreateGameView>();
            services.AddTransient<UpdateProfileView>();
            services.AddTransient<ChangePasswordView>();

            services.AddSingleton<Func<LoginView>>(provider => () => provider.GetRequiredService<LoginView>());
            services.AddSingleton<Func<CreateAccountView>>(provider => () => provider.GetRequiredService<CreateAccountView>());
            services.AddSingleton<Func<MainMenuView>>(provider => () => provider.GetRequiredService<MainMenuView>());
            services.AddSingleton<Func<JoinOrCreateGameView>>(provider => () => provider.GetRequiredService<JoinOrCreateGameView>());
            services.AddSingleton<Func<UpdateProfileView>>(provider => () => provider.GetRequiredService<UpdateProfileView>());
            services.AddSingleton<Func<ChangePasswordView>>(provider => () => provider.GetRequiredService<ChangePasswordView>());
            services.AddSingleton<Func<FriendManagerWindow>>(provider => () => provider.GetRequiredService<FriendManagerWindow>());

            services.AddSingleton<GameWindow>();

            serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
            mainWindow.Show();
        }
    }
}