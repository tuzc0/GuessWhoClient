using GuessWhoClient.MatchServiceRef;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Dtos
{
    public sealed class GamePlayParameters
    {
        public long MatchId { get; }
        public string MatchCode { get; }
        public IReadOnlyList<LobbyPlayerDto> Players { get; }

        public GamePlayParameters(long matchId, string matchCode, IReadOnlyList<LobbyPlayerDto> players)
        {
            MatchId = matchId;
            MatchCode = matchCode ?? string.Empty;
            Players = players ?? Array.Empty<LobbyPlayerDto>();
        }
    }
}
