using System;
using System.Windows.Threading;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;

namespace GuessWhoClient.Callbacks
{
    public sealed class MatchCallback : IMatchServiceCallback
    {
        private readonly Dispatcher dispatcher;
        private ILobbyClient lobbyClient;

        public MatchCallback(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void AttachLobby(ILobbyClient lobbyClient)
        {
            this.lobbyClient = lobbyClient ?? throw new ArgumentNullException(nameof(lobbyClient));
        }

        public void OnPlayerJoined(LobbyPlayerDto player)
        {
            dispatcher.Invoke(() =>
            {
                if (lobbyClient != null)
                {
                    lobbyClient.OnPlayerJoined(player);
                }
            });
        }

        public void OnPlayerLeft(LobbyPlayerDto player)
        {
            dispatcher.Invoke(() =>
            {
                if (lobbyClient != null)
                {
                    lobbyClient.OnPlayerLeft(player);
                }
            });
        }

        public void OnReadyChanged(LobbyPlayerDto player)
        {
            dispatcher.Invoke(() =>
            {
                if (lobbyClient != null)
                {
                    lobbyClient.OnReadyChanged(player);
                }
            });
        }
    }
}
