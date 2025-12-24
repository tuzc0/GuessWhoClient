using System;
using System.Threading.Tasks;
using GuessWhoClient.Domain.Models;

namespace GuessWhoClient.Interfaces
{
    public interface IMatchSessionController
    {
        long MatchId { get; }
        long CurrentUserId { get; }

        Task SubscribeLobbyAsync();
        Task UnsubscribeLobbyAsync();

        Task SetPlayerReadyAsync();
        Task StartMatchAsync();
        Task LeaveMatchAsync();

        Task ChooseSecretCharacterAsync(string characterId);
        Task EndMatchAsync(long winnerUserId);

        Task<string[]> GetMatchDeckAsync(long matchId, int numberOfCardsInDeck);

        Task CloseAsync();

        event Action<LobbyPlayer> PlayerJoined;
        event Action<LobbyPlayer> PlayerLeft;
        event Action<LobbyPlayer> ReadyChanged;
        event Action GameStarted;
        event Action<long, long> SecretCharacterChosen;  
        event Action<long> AllSecretCharactersChosen;    
    }
}
