using GuessWhoClient.Dtos;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Mappers;
using GuessWhoClient.MatchServiceRef;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Session.Controllers
{
    public sealed class MatchSessionController : IMatchSessionController, IDisposable
    {
        public event Action<ClientLobbyPlayerDto> PlayerJoined;
        public event Action<ClientLobbyPlayerDto> PlayerLeft;
        public event Action<ClientLobbyPlayerDto> ReadyChanged;
        public event Action GameStarted;
        public event Action<long, long> SecretCharacterChosen;
        public event Action<long> AllSecretCharactersChosen;

        private const string MATCH_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IMatchService";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MatchSessionController));

        private readonly MatchServiceClient matchServiceClient;

        public long MatchId { get; }
        public long CurrentUserId { get; }

        public MatchSessionController(long matchId, long currentUserId)
        {
            MatchId = matchId;
            CurrentUserId = currentUserId;

            var callbackInstance = new MatchServiceCallbackAdapter(this);
            var context = new InstanceContext(callbackInstance);

            matchServiceClient = new MatchServiceClient(context, MATCH_SERVICE_ENDPOINT_NAME);
        }

        public async Task SubscribeLobbyAsync()
        {
            Logger.InfoFormat("SubscribeLobbyAsync invoked. MatchId={0}, UserId={1}", MatchId,
                CurrentUserId);

            try
            {
                await matchServiceClient.SubscribeLobbyAsync(MatchId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("SubscribeLobbyAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("SubscribeLobbyAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("SubscribeLobbyAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("SubscribeLobbyAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("SubscribeLobbyAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public async Task UnsubscribeLobbyAsync()
        {
            Logger.InfoFormat("UnsubscribeLobbyAsync invoked. MatchId={0}, UserId={1}", MatchId,
                CurrentUserId);

            try
            {
                await matchServiceClient.UnsubscribeLobbyAsync(MatchId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("UnsubscribeLobbyAsync: service fault (best-effort).", ex);
                AbortIfFaulted();
            }
            catch (FaultException ex)
            {
                Logger.Warn("UnsubscribeLobbyAsync: fault exception (best-effort).", ex);
                AbortIfFaulted();
            }
            catch (TimeoutException ex)
            {
                Logger.Warn("UnsubscribeLobbyAsync: timeout (best-effort).", ex);
                AbortIfFaulted();
            }
            catch (CommunicationException ex)
            {
                Logger.Warn("UnsubscribeLobbyAsync: communication error (best-effort).", ex);
                AbortIfFaulted();
            }
            catch (Exception ex)
            {
                Logger.Warn("UnsubscribeLobbyAsync: unexpected error (best-effort).", ex);
                AbortIfFaulted();
            }
        }

        public async Task LeaveMatchAsync()
        {
            Logger.InfoFormat("LeaveMatchAsync invoked. MatchId={0}, UserId={1}", MatchId,
                CurrentUserId);

            try
            {
                var request = new LeaveMatchRequest
                {
                    MatchId = MatchId,
                    UserId = CurrentUserId
                };

                var response = await matchServiceClient.LeaveMatchAsync(request);

                Logger.InfoFormat(
                    "LeaveMatchAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    Logger.Warn("LeaveMatchAsync returned Success=false.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("LeaveMatchAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("LeaveMatchAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("LeaveMatchAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("LeaveMatchAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("LeaveMatchAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }

            await UnsubscribeLobbyAsync();
        }

        public async Task SetPlayerReadyAsync()
        {
            Logger.InfoFormat("SetPlayerReadyAsync invoked. MatchId={0}, UserId={1}", MatchId,
                CurrentUserId);

            try
            {
                var request = new SetPlayerReadyStatusRequest
                {
                    MatchId = MatchId,
                    UserId = CurrentUserId
                };

                var response = await matchServiceClient.SetPlayerReadyStatusAsync(request);

                Logger.InfoFormat("SetPlayerReadyStatusAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    Logger.Warn("SetPlayerReadyStatusAsync returned Success=false.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("SetPlayerReadyAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("SetPlayerReadyAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("SetPlayerReadyAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("SetPlayerReadyAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("SetPlayerReadyAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public async Task StartMatchAsync()
        {
            Logger.InfoFormat("StartMatchAsync invoked. MatchId={0}, UserId={1}", MatchId,
                CurrentUserId);

            try
            {
                var request = new StartMatchRequest
                {
                    MatchId = MatchId
                };

                var response = await matchServiceClient.StartMatchAsync(request);

                Logger.InfoFormat("StartMatchAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    Logger.Warn("StartMatchAsync returned Success=false.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("StartMatchAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("StartMatchAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("StartMatchAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("StartMatchAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("StartMatchAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public async Task ChooseSecretCharacterAsync(string characterId)
        {
            Logger.InfoFormat("ChooseSecretCharacterAsync invoked. MatchId={0}, UserId={1}, CharacterId={2}",
                MatchId, CurrentUserId, characterId);

            try
            {
                var request = new ChooseSecretCharacterRequest
                {
                    MatchId = MatchId,
                    UserId = CurrentUserId,
                    CharacterId = characterId
                };

                var response = await matchServiceClient.ChooseSecretCharacterAsync(request);

                Logger.InfoFormat("ChooseSecretCharacterAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    Logger.Warn("ChooseSecretCharacterAsync returned Success=false.");
                    throw new InvalidOperationException("No se pudo registrar el personaje secreto en el servidor.");
                }

            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("ChooseSecretCharacterAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("ChooseSecretCharacterAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("ChooseSecretCharacterAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("ChooseSecretCharacterAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("ChooseSecretCharacterAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public async Task EndMatchAsync(long winnerUserId)
        {
            Logger.InfoFormat("EndMatchAsync invoked. MatchId={0}, WinnerUserId={1}",
                MatchId, winnerUserId);

            try
            {
                var request = new EndMatchRequest
                {
                    MatchId = MatchId,
                    WinnerUserId = winnerUserId
                };

                var response = await matchServiceClient.EndMatchAsync(request);

                Logger.InfoFormat("EndMatchAsync response received. Success={0}",
                    response.Success);

                if (!response.Success)
                {
                    Logger.Warn("EndMatchAsync returned Success=false.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("EndMatchAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("EndMatchAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("EndMatchAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("EndMatchAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("EndMatchAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public async Task<string[]> GetMatchDeckAsync(long matchId, int numberOfCardsInDeck)
        {
            Logger.InfoFormat(
                "GetMatchDeckAsync invoked. MatchId={0}, NumberOfCardsInDeck={1}",
                matchId,
                numberOfCardsInDeck);

            try
            {
                var request = new GetMatchDeckRequest
                {
                    MatchId = matchId,
                    NumberOfCardsInDeck = numberOfCardsInDeck
                };

                var response = await matchServiceClient.GetMatchDeckAsync(request);

                int characterCount = response?.CharacterIds == null
                    ? 0
                    : response.CharacterIds.Length;

                Logger.InfoFormat(
                    "GetMatchDeckAsync response received. MatchId={0}, CharacterCount={1}",
                    matchId,
                    characterCount);

                return response?.CharacterIds ?? Array.Empty<string>();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("GetMatchDeckAsync: service fault.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (FaultException ex)
            {
                Logger.Error("GetMatchDeckAsync: fault exception.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("GetMatchDeckAsync: timeout.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("GetMatchDeckAsync: communication error.", ex);
                AbortIfFaulted();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("GetMatchDeckAsync: unexpected error.", ex);
                AbortIfFaulted();
                throw;
            }
        }

        public Task CloseAsync()
        {
            CloseClient();
            return Task.CompletedTask;
        }

        private void AbortIfFaulted()
        {
            if (matchServiceClient.State == CommunicationState.Faulted)
            {
                matchServiceClient.Abort();
            }
        }

        private void CloseClient()
        {
            try
            {
                if (matchServiceClient.State == CommunicationState.Faulted)
                {
                    matchServiceClient.Abort();
                }
                else
                {
                    matchServiceClient.Close();
                }
            }
            catch
            {
                matchServiceClient.Abort();
            }
        }

        public void Dispose()
        {
            CloseClient();
        }

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
        private sealed class MatchServiceCallbackAdapter : IMatchServiceCallback
        {
            private readonly MatchSessionController matchSessionController;

            public MatchServiceCallbackAdapter(MatchSessionController matchSessionController)
            {
                this.matchSessionController = matchSessionController
                    ?? throw new ArgumentNullException(nameof(matchSessionController));
            }

            public void OnPlayerJoined(LobbyPlayerDto player)
            {
                if (player == null)
                {
                    return;
                }

                ClientLobbyPlayerDto clientDto = LobbyPlayerMapper.ToClient(player);

                Logger.InfoFormat("Callback OnPlayerJoined received. MatchId={0}, UserId={1}",
                    clientDto.MatchId, clientDto.UserId, clientDto.Avatar);

                matchSessionController.PlayerJoined?.Invoke(clientDto);
            }

            public void OnPlayerLeft(LobbyPlayerDto player)
            {
                if (player == null)
                {
                    return;
                }

                ClientLobbyPlayerDto clientDto = LobbyPlayerMapper.ToClient(player);

                Logger.InfoFormat("Callback OnPlayerLeft received. MatchId={0}, UserId={1}",
                    clientDto.MatchId, clientDto.UserId);

                matchSessionController.PlayerLeft?.Invoke(clientDto);
            }

            public void OnReadyChanged(LobbyPlayerDto player)
            {
                if (player == null)
                {
                    return;
                }

                ClientLobbyPlayerDto clientDto = LobbyPlayerMapper.ToClient(player);

                Logger.InfoFormat("Callback OnReadyChanged received. MatchId={0}, UserId={1}, IsReady={2}",
                    clientDto.MatchId, clientDto.UserId, clientDto.IsReady);

                matchSessionController.ReadyChanged?.Invoke(clientDto);
            }

            public void OnSecretCharacterChosen(long matchId, long userId)
            {
                Logger.InfoFormat("Callback OnSecretCharacterChosen received. MatchId={0}, UserId={1}",
                    matchId, userId);

                matchSessionController.SecretCharacterChosen?.Invoke(matchId, userId);
            }

            public void OnAllSecretCharactersChosen(long matchId)
            {
                Logger.InfoFormat("Callback OnAllSecretCharactersChosen received. MatchId={0}",
                    matchId);

                matchSessionController.AllSecretCharactersChosen?.Invoke(matchId);
            }

            public void OnGameStarted(long matchId)
            {
                Logger.InfoFormat("Callback OnGameStarted received. MatchId={0}", matchId);

                matchSessionController.GameStarted?.Invoke();
            }

            public void OnGameEnded(long matchId, long winnerUserId)
            {
                Logger.InfoFormat("Callback OnGameEnded received. MatchId={0}, WinnerUserId={1}",
                    matchId, winnerUserId);
            }
        }
    }
}
