using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Leaderboard
{
    public interface ILeaderboardAppService
    {
        Task<WcfCallResult<GetLeaderboardResponse>> GetGlobalLeaderboardAsync(GetLeaderboardRequest request);
    }
}