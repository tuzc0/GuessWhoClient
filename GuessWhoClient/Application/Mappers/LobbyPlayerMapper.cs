using GuessWhoClient.Domain.Models;
using GuessWhoClient.MatchServiceRef;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Application.Mappers
{
    internal static class LobbyPlayerMapper
    {
        internal static LobbyPlayer ToDomain(LobbyPlayerDto servicePlayer)
        {
            if (servicePlayer == null)
            {
                throw new ArgumentNullException(nameof(servicePlayer));
            }

            return new LobbyPlayer(
                servicePlayer.MatchId,
                servicePlayer.UserId,
                servicePlayer.DisplayName,
                servicePlayer.AvatarId,
                servicePlayer.SlotNumber,
                servicePlayer.IsReady,
                servicePlayer.IsHost);
        }

        internal static IReadOnlyList<LobbyPlayer> ToDomainList(LobbyPlayerDto[] servicePlayers)
        {
            if (servicePlayers == null || servicePlayers.Length == 0)
            {
                return Array.Empty<LobbyPlayer>();
            }

            var players = new LobbyPlayer[servicePlayers.Length];

            for (int index = 0; index < servicePlayers.Length; index++)
            {
                players[index] = ToDomain(servicePlayers[index]);
            }

            return players;
        }
    }
}
