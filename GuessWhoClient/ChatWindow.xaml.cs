using GuessWhoClient.ChatServiceRef;
using System.ServiceModel;
using System.Windows;

namespace GuessWhoClient
{
    public partial class ChatWindow : Window
    {
        private IChatService chatProxy;

        public ChatWindow()
        {
            InitializeComponent();

            var callback = new ChatCallbackWrapper(this);
            var context = new InstanceContext(callback);
            var factory = new DuplexChannelFactory<IChatService>(context, "NetTcpBinding_IChatService");
            chatProxy = factory.CreateChannel();

            chatProxy.Join("UsuarioPrueba");
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                chatProxy.SendMessage("UsuarioPrueba", txtMessage.Text);
                txtMessage.Clear();
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
    }
  
    public class ChatCallbackWrapper : IChatServiceCallback
    {
        private readonly ChatWindow window;

        public ChatCallbackWrapper(ChatWindow window)
        {
            this.window = window;
        }

        public void ReceiveMessage(string user, string message)
        {
            window.AddMessage(user, message);
        }
    }
}
