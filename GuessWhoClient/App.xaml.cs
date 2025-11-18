using System.Windows;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]
namespace GuessWhoClient
{
    public partial class App : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));

        private const string APPLICATION_STARTING_MESSAGE = "Client application starting";
        private const string APPLICATION_STARTED_MESSAGE = "Client application started";
        private const string APPLICATION_UNHANDLED_ERROR_MESSAGE = "Unhandled application error.";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.Info(APPLICATION_STARTING_MESSAGE);
            Logger.Info(APPLICATION_STARTED_MESSAGE);

            DispatcherUnhandledException += (_, args) =>
            {
                Logger.Fatal(APPLICATION_UNHANDLED_ERROR_MESSAGE);
            };
        }
    }
}
