using GuessWhoClient.ChatServiceRef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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