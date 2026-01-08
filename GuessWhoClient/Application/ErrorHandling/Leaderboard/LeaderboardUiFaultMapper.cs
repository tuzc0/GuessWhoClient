using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System.Collections.Generic;

namespace GuessWhoClient.Application.ErrorHandling.Leaderboard
{
    public sealed class LeaderboardUiFaultMapper : IUiFaultMapper
    {
        private static readonly HashSet<string> KnownFaults = new HashSet<string>
        {
            LeaderboardFaultKeys.CODE_REQUEST_NULL,
            LeaderboardFaultKeys.CODE_INVALID_TOP_N,
            LeaderboardFaultKeys.CODE_USER_NOT_FOUND,
            LeaderboardFaultKeys.CODE_UNEXPECTED_ERROR
        };

        public UiKeyMapping Map(string faultCode)
        {
            if (faultCode != null && KnownFaults.Contains(faultCode))
            {
                return new UiKeyMapping(true, faultCode);
            }

            return new UiKeyMapping(false, null);
        }
    }
}