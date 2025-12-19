using System;
using System.Windows.Threading;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;

using ClientLobbyPlayerDto = GuessWhoClient.Dtos.ClientLobbyPlayerDto;
using ServiceLobbyPlayerDto = GuessWhoClient.MatchServiceRef.LobbyPlayerDto;

namespace GuessWhoClient.Callbacks
{
    public sealed class MatchCallback : IMatchServiceCallback
    {
        private readonly Dispatcher dispatcher;
        private ILobbyClient lobbyClient;

        public MatchCallback(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void AttachLobby(ILobbyClient lobbyClient)
        {
            this.lobbyClient = lobbyClient ?? throw new ArgumentNullException(nameof(lobbyClient));
        }

        public void OnPlayerJoined(ServiceLobbyPlayerDto player)
        {
            if (lobbyClient == null || player == null)
            {
                return;
            }

            ClientLobbyPlayerDto clientPlayer = MapToClientDto(player);

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnPlayerJoined(clientPlayer);
            });
        }

        public void OnPlayerLeft(ServiceLobbyPlayerDto player)
        {
            if (lobbyClient == null || player == null)
            {
                return;
            }

            ClientLobbyPlayerDto clientPlayer = MapToClientDto(player);

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnPlayerLeft(clientPlayer);
            });
        }

        public void OnReadyChanged(ServiceLobbyPlayerDto player)
        {
            if (lobbyClient == null || player == null)
            {
                return;
            }

            ClientLobbyPlayerDto clientPlayer = MapToClientDto(player);

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnReadyChanged(clientPlayer);
            });
        }

        public void OnGameStarted(long matchId)
        {
            if (lobbyClient == null)
            {
                return;
            }

            dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    lobbyClient.OnGameStarted();
                }));
        }

        public void OnSecretCharacterChosen(long matchId, long userId)
        {
            if (lobbyClient == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnSecretCharacterChosen(matchId, userId);
            });
        }

        public void OnAllSecretCharactersChosen(long matchId)
        {
            if (lobbyClient == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnAllSecretCharactersChosen(matchId);
            });
        }

        public void OnGameEnded(long matchId, long winnerUserId)
        {
            if (lobbyClient == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                lobbyClient.OnGameEnded(matchId, winnerUserId);
            });
        }

        private static ClientLobbyPlayerDto MapToClientDto(ServiceLobbyPlayerDto servicePlayer)
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
                SlotNumber = servicePlayer.SlotNumber,
                IsReady = servicePlayer.IsReady,
                IsHost = servicePlayer.IsHost
            };
        }
    }
}
