using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.MatchServiceRef;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GuessWhoClient.Infraestructure.Match
{
    public sealed class MatchHub : IDisposable, IMatchClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MatchHub));

        private const string EMPTY = "";
        private const long INVALID_ID = 0;

        private const string LOG_CTX_CONNECT = "MatchHub.Connect";
        private const string LOG_CTX_CREATE_MATCH = "MatchHub.CreateMatch";
        private const string LOG_CTX_JOIN_MATCH = "MatchHub.JoinMatch";
        private const string LOG_CTX_SET_VISIBILITY = "MatchHub.SetMatchVisibility";
        private const string LOG_CTX_START_MATCH = "MatchHub.StartMatch";
        private const string LOG_CTX_SET_READY = "MatchHub.SetPlayerReadyStatus";
        private const string LOG_CTX_LEAVE_MATCH = "MatchHub.LeaveMatch";
        private const string LOG_CTX_SUBSCRIBE = "MatchHub.SubscribeLobby";
        private const string LOG_CTX_UNSUBSCRIBE = "MatchHub.UnsubscribeLobby";
        private const string LOG_CTX_SEARCH_PUBLIC = "MatchHub.SearchPublicMatch";

        private const string CODE_INVALID_ARGS = "MATCH_INVALID_ARGS";
        private const string KEY_INVALID_ARGS = "Match.InvalidArgs";

        private const string KEY_NOT_CONNECTED = "Match.NotConnected";

        private readonly Dispatcher dispatcher;
        private readonly MatchCallback callback;
        private readonly DuplexWcfCallExecutor duplexCallExecutor;

        private MatchServiceClient client;

        public event Action<LobbyPlayerDto> PlayerJoined
        {
            add { callback.PlayerJoined += value; }
            remove { callback.PlayerJoined -= value; }
        }

        public event Action<LobbyPlayerDto> PlayerLeft
        {
            add { callback.PlayerLeft += value; }
            remove { callback.PlayerLeft -= value; }
        }

        public event Action<LobbyPlayerDto> ReadyChanged
        {
            add { callback.ReadyChanged += value; }
            remove { callback.ReadyChanged -= value; }
        }

        public event Action<long> GameStarted
        {
            add { callback.GameStarted += value; }
            remove { callback.GameStarted -= value; }
        }

        public MatchHub(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            callback = new MatchCallback(this.dispatcher);
            duplexCallExecutor = new DuplexWcfCallExecutor();
        }

        public bool IsConnected => TryGetConnectedClient(out _);

        public async Task<WcfCallResult<bool>> ConnectAsync()
        {
            if (TryGetConnectedClient(out _))
            {
                return WcfCallResult<bool>.Ok(true);
            }

            await DisconnectAsync();

            var instanceContext = new InstanceContext(callback);
            client = new MatchServiceClient(instanceContext, WcfEndpointNames.MATCH_SERVICE);

            WcfCallResult<bool> openResult = await WcfCallErrorHelper.ExecuteAsync(
                () =>
                {
                    client.Open();
                    return Task.FromResult(true);
                },
                Logger,
                LOG_CTX_CONNECT);

            if (!openResult.IsSuccess)
            {
                await DisconnectAsync();
            }

            return openResult;
        }


        public async Task<WcfCallResult<CreateMatchResponse>> CreateMatchAsync(long profileId)
        {
            if (profileId <= INVALID_ID)
            {
                return WcfCallResult<CreateMatchResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return NotConnected<CreateMatchResponse>();
            }

            var request = new CreateMatchRequest
            {
                ProfileId = profileId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, CreateMatchResponse>(
                openClient,
                c => Task.Run(() => c.CreateMatch(request)),
                Logger,
                LOG_CTX_CREATE_MATCH);
        }

        public async Task<WcfCallResult<JoinMatchResponse>> JoinMatchAsync(string matchCode, long userId)
        {
            string safeCode = (matchCode ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeCode) || userId <= INVALID_ID)
            {
                return WcfCallResult<JoinMatchResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return NotConnected<JoinMatchResponse>();
            }

            var request = new JoinMatchRequest
            {
                MatchCode = safeCode,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, JoinMatchResponse>(
                openClient,
                c => Task.Run(() => c.JoinMatch(request)),
                Logger,
                LOG_CTX_JOIN_MATCH);
        }

        public async Task<WcfCallResult<BasicResponse>> SetMatchVisibilityAsync(long matchId, long userId, bool isPrivate)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, KEY_NOT_CONNECTED);
            }

            var request = new SetMatchVisibilityRequest
            {
                MatchId = matchId,
                UserId = userId,
                IsPrivate = isPrivate
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.SetMatchVisibility(request)),
                Logger,
                LOG_CTX_SET_VISIBILITY);
        }

        public async Task<WcfCallResult<BasicResponse>> StartMatchAsync(long matchId, long userId)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, KEY_NOT_CONNECTED);
            }

            var request = new StartMatchRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.StartMatch(request)),
                Logger,
                LOG_CTX_START_MATCH);
        }

        public async Task<WcfCallResult<SearchPublicMatchResponse>> SearchPublicMatchAsync(string matchCode)
        {
            string safeCode = (matchCode ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeCode))
            {
                return WcfCallResult<SearchPublicMatchResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return WcfCallResult<SearchPublicMatchResponse>.Fail(WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, KEY_NOT_CONNECTED);
            }

            var request = new SearchPublicMatchRequest { MatchCode = safeCode };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, SearchPublicMatchResponse>(
                openClient,
                c => Task.Run(() => c.SearchPublicMatch(request)),
                Logger,
                LOG_CTX_SEARCH_PUBLIC);
        }

        public async Task<WcfCallResult<BasicResponse>> SetPlayerReadyStatusAsync(long matchId, long userId)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, KEY_NOT_CONNECTED);
            }

            var request = new SetPlayerReadyStatusRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.SetPlayerReadyStatus(request)),
                Logger,
                LOG_CTX_SET_READY);
        }

        public async Task<WcfCallResult<BasicResponse>> LeaveMatchAsync(long matchId, long userId)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, KEY_NOT_CONNECTED);
            }

            var request = new LeaveMatchRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.LeaveMatch(request)),
                Logger,
                LOG_CTX_LEAVE_MATCH);
        }

        public async Task<WcfCallResult<BasicResponse>> SubscribeLobbyAsync(long matchId, long userId)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return NotConnected<BasicResponse>();
            }

            var request = new SubscribeLobbyRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.SubscribeLobby(request)),
                Logger,
                LOG_CTX_SUBSCRIBE);
        }

        public async Task<WcfCallResult<BasicResponse>> UnsubscribeLobbyAsync(long matchId, long userId)
        {
            if (matchId <= INVALID_ID || userId <= INVALID_ID)
            {
                return WcfCallResult<BasicResponse>.Fail(CODE_INVALID_ARGS, KEY_INVALID_ARGS);
            }

            if (!TryGetConnectedClient(out MatchServiceClient openClient))
            {
                return NotConnected<BasicResponse>();
            }

            var request = new UnsubscribeLobbyRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await duplexCallExecutor.CallAsync<MatchServiceClient, BasicResponse>(
                openClient,
                c => Task.Run(() => c.UnsubscribeLobby(request)),
                Logger,
                LOG_CTX_UNSUBSCRIBE);
        }

        public async Task DisconnectAsync()
        {
            if (client == null)
            {
                return;
            }

            MatchServiceClient toClose = client;
            client = null;

            await ServiceClientGuard.CloseSafelyAsync(toClose);
        }

        public void Dispose()
        {
            _ = DisconnectAsync();
        }

        private bool TryGetConnectedClient(out MatchServiceClient openClient)
        {
            openClient = client;

            if (openClient == null)
            {
                return false;
            }

            CommunicationState state = openClient.State;

            if (state == CommunicationState.Faulted ||
                state == CommunicationState.Closed ||
                state == CommunicationState.Closing)
            {
                return false;
            }

            return true;
        }

        private static WcfCallResult<T> NotConnected<T>()
        {
            return WcfCallResult<T>.Fail(
                WcfTechnicalFaultCodes.CLIENT_NOT_CONNECTED, 
                KEY_NOT_CONNECTED);
        }
    }
}
