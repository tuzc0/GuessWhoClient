using GuessWhoClient.MatchServiceRef;

namespace GuessWhoClient.Interfaces
{
    public interface ILobbyClient
    {
        void OnPlayerJoined(LobbyPlayerDto player);
        void OnPlayerLeft(LobbyPlayerDto player);
        void OnReadyChanged(LobbyPlayerDto player);
    }

}
