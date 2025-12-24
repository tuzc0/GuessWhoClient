using GuessWhoClient.Application.Services;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoClient.Presentation.Views.Windows;
using GuessWhoClient.ViewModels;
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

            services.AddSingleton<GameWindow>();

            serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<GameWindow>();
            mainWindow.Show();
        }
    }
}
