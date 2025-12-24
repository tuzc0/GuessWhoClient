using GuessWhoClient.Presentation.Views.Windows;

namespace GuessWhoClient.Alerts
{
    public static class GameMessageBox
    {
        public static void ShowInfo(string message, string title)
        {
            Show(message, title, GameMessageType.Info);
        }

        public static void ShowWarning(string message, string title)
        {
            Show(message, title, GameMessageType.Warning);
        }

        public static void ShowError(string message, string title)
        {
            Show(message, title, GameMessageType.Error);
        }

        private static void Show(string message, string title, GameMessageType type)
        {
            var window = new GameMessageWindow(message, title, type)
            {
                Owner = System.Windows.Application.Current?.MainWindow
            };

            window.ShowDialog();
        }
    }
}
