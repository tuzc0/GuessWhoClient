using GuessWhoClient.Dtos;
using System;
using System.Collections.Generic;

public sealed class GamePlayParameters
{
    public long MatchId { get; }
    public string MatchCode { get; }
    public IReadOnlyList<ClientLobbyPlayerDto> Players { get; }

    public GamePlayParameters(long matchId, string matchCode, IReadOnlyList<ClientLobbyPlayerDto> players)
    {
        MatchId = matchId;
        MatchCode = matchCode;
        Players = players ?? Array.Empty<ClientLobbyPlayerDto>();
    }
}
