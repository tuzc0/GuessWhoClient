using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    public interface ILeaderboardServiceClient
    {
        Task<WcfCallResult<GetLeaderboardResponse>> GetGlobalLeaderboardAsync(GetLeaderboardRequest request);
    }
}