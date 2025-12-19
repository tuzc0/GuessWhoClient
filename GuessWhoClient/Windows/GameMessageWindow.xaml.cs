using System;
using System.Windows;

namespace GuessWhoClient.Windows
{
    public enum GameMessageType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public partial class GameMessageWindow : Window
    {
        private const string DEFAULT_TITLE = "Message";

        public string MessageTitle { get; }
        public string MessageText { get; }
        public GameMessageType MessageType { get; }

        public GameMessageWindow(
            string messageText,
            string messageTitle,
            GameMessageType messageType)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                throw new ArgumentException("Message text cannot be null or empty.", nameof(messageText));
            }

            MessageText = messageText;
            MessageTitle = string.IsNullOrWhiteSpace(messageTitle)
                ? DEFAULT_TITLE
                : messageTitle;
            MessageType = messageType;

            InitializeComponent();

            DataContext = this;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
