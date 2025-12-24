using System.Windows;

namespace GuessWhoClient.Alerts
{
    public sealed class MessageBoxAlertService : IAlertService
    {
        public void Warn(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Info(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);

        public void Error(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleError"), MessageBoxButton.OK, MessageBoxImage.Error);

        private static string GetLocalizedText(string key) =>
            Globalization.LocalizationProvider.Instance[key];
    }
}
