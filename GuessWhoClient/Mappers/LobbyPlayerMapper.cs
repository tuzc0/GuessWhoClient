using GuessWhoClient.Dtos;
using GuessWhoClient.MatchServiceRef;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Mappers
{
    public static class LobbyPlayerMapper
    {
        public static ClientLobbyPlayerDto ToClient(LobbyPlayerDto servicePlayer)
        {
            if (servicePlayer == null)
            {
                throw new ArgumentNullException(nameof(servicePlayer));
            }

            return new ClientLobbyPlayerDto
            {
                MatchId = servicePlayer.MatchId,
                UserId = servicePlayer.UserId,
                DisplayName = servicePlayer.DisplayName,
                Avatar = servicePlayer.AvatarId,
                SlotNumber = servicePlayer.SlotNumber,
                IsReady = servicePlayer.IsReady,
                IsHost = servicePlayer.IsHost
            };
        }

        public static IReadOnlyList<ClientLobbyPlayerDto> ToClientList(LobbyPlayerDto[] servicePlayers)
        {
            if (servicePlayers == null || servicePlayers.Length == 0)
            {
                return Array.Empty<ClientLobbyPlayerDto>();
            }

            var result = new ClientLobbyPlayerDto[servicePlayers.Length];

            for (int index = 0; index < servicePlayers.Length; index ++)
            {
                LobbyPlayerDto servicePlayer = servicePlayers[index];
                result[index] = ToClient(servicePlayer);
            }

            return result; 
        }
    }
}
