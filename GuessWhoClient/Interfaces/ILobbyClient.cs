using GuessWhoClient.MatchServiceRef;

namespace GuessWhoClient.Interfaces
{
    public interface ILobbyClient
    {
        void OnPlayerJoined(LobbyPlayerDto player);

        void OnPlayerLeft(LobbyPlayerDto player);

        void OnReadyChanged(LobbyPlayerDto player);

        void OnGameStarted();

        void OnSecretCharacterChosen(long matchId, long userId);

        void OnAllSecretCharactersChosen(long matchId);

        void OnGameEnded(long matchId, long winnerUserId);
    }
}
