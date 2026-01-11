using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public interface IGameLobbyViewModelFactory
    {
        GameLobbyViewModel CreateFromJoin(JoinMatchResponse response, long currentUserId);

        GameLobbyViewModel CreateFromCreate(CreateMatchResponse created, string createdMatchCode, long currentUserId);
    }
}
