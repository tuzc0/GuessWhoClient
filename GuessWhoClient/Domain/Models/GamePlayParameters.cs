using GuessWhoClient.Domain.Models;
using System;
using System.Collections.Generic;

public sealed class GamePlayParameters
{
    public long MatchId { get; }
    public string MatchCode { get; }
    public IReadOnlyList<LobbyPlayer> Players { get; }

    public GamePlayParameters(long matchId, string matchCode, IReadOnlyList<LobbyPlayer> players)
    {
        MatchId = matchId;
        MatchCode = matchCode;
        Players = players ?? Array.Empty<LobbyPlayer>();
    }
}
