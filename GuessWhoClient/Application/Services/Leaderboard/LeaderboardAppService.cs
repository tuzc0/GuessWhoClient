using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.LeaderboardServiceRef;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Leaderboard
{
    internal sealed class LeaderboardAppService : ILeaderboardAppService
    {
        private readonly ILeaderboardServiceClient leaderboardServiceClient;

        public LeaderboardAppService(ILeaderboardServiceClient leaderboardServiceClient)
        {
            this.leaderboardServiceClient = leaderboardServiceClient ??
                throw new ArgumentNullException(nameof(leaderboardServiceClient));
        }

        public Task<WcfCallResult<GetLeaderboardResponse>> GetGlobalLeaderboardAsync(GetLeaderboardRequest request)
        {
            return leaderboardServiceClient.GetGlobalLeaderboardAsync(request);
        }
    }
}