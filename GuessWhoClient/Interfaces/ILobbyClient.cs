using GuessWhoClient.Dtos;

namespace GuessWhoClient.Interfaces
{
    public interface ILobbyClient
    {
        void OnPlayerJoined(ClientLobbyPlayerDto player);

        void OnPlayerLeft(ClientLobbyPlayerDto player);

        void OnReadyChanged(ClientLobbyPlayerDto player);

        void OnGameStarted();

        void OnSecretCharacterChosen(long matchId, long userId);

        void OnAllSecretCharactersChosen(long matchId);

        void OnGameEnded(long matchId, long winnerUserId);
    }
}
