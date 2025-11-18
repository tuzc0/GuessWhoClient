using GuessWhoClient.ChatServiceRef;
using System;

namespace GuessWhoClient.Callbacks
{

    public class ChatCallback : IChatServiceCallback
    {
        public void ReceiveMessage(string user, string message)
        {
            Console.WriteLine($"{user}: {message}");
        }
    }
}
