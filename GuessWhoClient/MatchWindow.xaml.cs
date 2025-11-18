using System;
using System.Windows;
using GuessWhoClient.ChatServiceRef;
using System.ServiceModel;

namespace GuessWhoClient
{
    public partial class MatchWindow : Window
    {
        private IChatService chatProxy;
        private string playerName;

        public MatchWindow()
        {
            InitializeComponent();
            this.playerName = "Player";
            InitializeChat();
        }

        public MatchWindow(string playerName) : this()
        {
            this.playerName = playerName;
        }

        public class MatchChatCallback : IChatServiceCallback
        {
            private readonly MatchWindow window;

            public MatchChatCallback(MatchWindow window)
            {
                this.window = window;
            }

            public void ReceiveMessage(string user, string message)
            {
                window.AddMessage(user, message);
            }
        }

        private void InitializeChat()
        {
            try
            {
                var callback = new MatchChatCallback(this);
                var context = new InstanceContext(callback);
                var factory = new DuplexChannelFactory<IChatService>(context, "NetTcpBinding_IChatService");
                chatProxy = factory.CreateChannel();

                chatProxy.Join(playerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                try
                {
                    chatProxy.SendMessage(playerName, txtMessage.Text);
                    txtMessage.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error enviando mensaje: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void AddMessage(string user, string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtChat.AppendText($"{user}: {message}\n");
                txtChat.ScrollToEnd();
            });
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                chatProxy?.Leave(playerName);
            }
            catch
            {

            }
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}