using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Match
{
    public interface IMatchClient : IDisposable
    {
        event Action<LobbyPlayerDto> PlayerJoined;
        event Action<LobbyPlayerDto> PlayerLeft;
        event Action<LobbyPlayerDto> ReadyChanged;
        event Action<long> GameStarted;

        bool IsConnected { get; }

        Task<WcfCallResult<bool>> ConnectAsync();

        Task<WcfCallResult<CreateMatchResponse>> CreateMatchAsync(long profileId);
        Task<WcfCallResult<JoinMatchResponse>> JoinMatchAsync(string matchCode, long userId);

        Task<WcfCallResult<BasicResponse>> SubscribeLobbyAsync(long matchId, long userId);
        Task<WcfCallResult<BasicResponse>> UnsubscribeLobbyAsync(long matchId, long userId);

        Task<WcfCallResult<BasicResponse>> SetPlayerReadyStatusAsync(long matchId, long userId);
        Task<WcfCallResult<BasicResponse>> LeaveMatchAsync(long matchId, long userId);

        Task<WcfCallResult<BasicResponse>> SetMatchVisibilityAsync(long matchId, long userId, bool isPrivate);
        Task<WcfCallResult<BasicResponse>> StartMatchAsync(long matchId, long callerId);
        Task DisconnectAsync();
    }
}
