using GuessWhoClient.Globalization;
using System.Windows;

namespace GuessWhoClient.Services.Alerts
{
    public sealed class MessageBoxAlertService : IAlertService
    {
        private const string KEY_TITLE_WARNING = "UiTitleWarning";
        private const string KEY_TITLE_INFO = "UiTitleInfo";
        private const string KEY_TITLE_ERROR = "UiTitleError";

        public void Warn(string message) => Warn(message, GetLocalizedText(KEY_TITLE_WARNING));
        public void Info(string message) => Info(message, GetLocalizedText(KEY_TITLE_INFO));
        public void Error(string message) => Error(message, GetLocalizedText(KEY_TITLE_ERROR));

        public void Warn(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Info(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);

        public void Error(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);

        private static string GetLocalizedText(string key) => LocalizationProvider.Instance[key];
    }
}
