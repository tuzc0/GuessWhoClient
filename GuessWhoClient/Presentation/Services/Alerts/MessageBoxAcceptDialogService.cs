using System.Windows;

namespace GuessWhoClient.Presentation.Services.Alerts
{
    public sealed class MessageBoxAcceptDialogService : IAcceptDialogService
    {
        public void Accept(string message, string title)
        {
            MessageBox.Show(
                message ?? string.Empty,
                title ?? string.Empty,
                MessageBoxButton.OK,
                MessageBoxImage.None);
        }
    }
}
