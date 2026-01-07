using System.Windows;

namespace GuessWhoClient.Services.Alerts
{
    public sealed class MessageBoxAlertService : IAlertService
    {
        public void Error(string message)
        {
            MessageBox.Show(message ?? string.Empty, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public void Ok(string message, string title) 
        { 
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public void Warn(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Info(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);

        public void Error(string message, string title) =>
            MessageBox.Show(message ?? string.Empty, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
