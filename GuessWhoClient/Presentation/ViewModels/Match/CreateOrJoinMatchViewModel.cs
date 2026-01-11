using GuessWhoClient.Assets;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.ErrorHandling.Mapper;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class CreateOrJoinViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreateOrJoinViewModel));

        private const string EMPTY = "";
        private const long INVALID_ID = 0;

        private const int MATCH_CODE_MAX_LENGTH = 6;

        private const string PROP_MATCH_CODE_INPUT = nameof(MatchCodeInput);

        private const string KEY_UI_GENERIC_ERROR = "UiGenericError";
        private const string KEY_MATCH_CREATE_FAILED = "Match.CreateFailed";
        private const string KEY_MATCH_JOIN_FAILED = "Match.JoinFailed";
        private const string KEY_MATCH_CODE_REQUIRED = "Match.CodeRequired";
        private const string KEY_MATCH_INVALID_ARGS = "Match.InvalidArgs";

        private const byte DEFAULT_VISIBILITY_PRIVATE = 2;
        private const byte DEFAULT_MODE_CLASSIC = 1;
        private const byte DEFAULT_STATUS_LOBBY = 1;

        private const byte HOST_SLOT_NUMBER = 1;

        private const string SESSION_PROP_DISPLAY_NAME = "DisplayName";
        private const string SESSION_PROP_AVATAR_ID = "AvatarId";
        private const string SESSION_PROP_AVATAR = "Avatar";
        private const string SESSION_PROP_AVATAR_KEY = "AvatarKey";

        private readonly MatchHub matchHub;
        private readonly IAvatarPathResolver avatarPathResolver;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly Func<string, string> localize;
        private readonly SessionContext sessionContext;

        private readonly long profileId;
        private readonly long userId;

        private readonly DataErrors dataErrors = new DataErrors();

        private string matchCodeInput;
        private string uiMessage;

        public CreateOrJoinViewModel(
            MatchHub matchHub,
            IAvatarPathResolver avatarPathResolver,
            SessionContext sessionContext,
            long profileId,
            long userId,
            IUiFaultMapper uiFaultMapper,
            Func<string, string> localize)
        {
            this.matchHub = matchHub ?? throw new ArgumentNullException(nameof(matchHub));
            this.avatarPathResolver = avatarPathResolver ?? throw new ArgumentNullException(nameof(avatarPathResolver));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            this.uiFaultMapper = uiFaultMapper ?? throw new ArgumentNullException(nameof(uiFaultMapper));
            this.localize = localize ?? throw new ArgumentNullException(nameof(localize));

            this.profileId = profileId;
            this.userId = userId;

            dataErrors.ErrorsChanged += (s, e) =>
            {
                ErrorsChanged?.Invoke(this, e);
                RaiseCommandStates();
            };

            CreateMatchCommand = new AsyncRelayCommand(CreateMatchAsync, CanCreateMatch);
            JoinMatchCommand = new AsyncRelayCommand(JoinMatchAsync, CanJoinMatch);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke(), () => !IsBusy);
        }

        public event Action<GameLobbyViewModel> LobbyRequested;
        public event Action BackRequested;

        public int MatchCodeMaxLength => MATCH_CODE_MAX_LENGTH;

        public ICommand CreateMatchCommand { get; }
        public ICommand JoinMatchCommand { get; }
        public ICommand BackCommand { get; }

        public string MatchCodeInput
        {
            get => matchCodeInput;
            set
            {
                string safe = value ?? EMPTY;

                if (safe.Length > MATCH_CODE_MAX_LENGTH)
                {
                    safe = safe.Substring(0, MATCH_CODE_MAX_LENGTH);
                }

                if (SetProperty(ref matchCodeInput, safe))
                {
                    ValidateJoinInputs();
                }
            }
        }

        public string UiMessage
        {
            get => uiMessage;
            private set => SetProperty(ref uiMessage, value ?? EMPTY);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => dataErrors.HasErrors;

        public IEnumerable GetErrors(string propertyName) => dataErrors.GetErrors(propertyName);

        protected override void OnIsBusyChanged(string propertyName) => RaiseCommandStates();

        private bool CanCreateMatch() => !IsBusy && profileId > INVALID_ID && userId > INVALID_ID;

        private bool CanJoinMatch() => !IsBusy && !HasErrors && userId > INVALID_ID;

        private async Task CreateMatchAsync()
        {
            ClearUiMessage();

            if (profileId <= INVALID_ID || userId <= INVALID_ID)
            {
                SetUiMessage(KEY_MATCH_INVALID_ARGS);
                return;
            }

            try
            {
                IsBusy = true;

                var connect = await matchHub.ConnectAsync();
                if (!connect.IsSuccess)
                {
                    SetUiMessage(MapFaultOrServerKey(connect.FaultCode, connect.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                var created = await matchHub.CreateMatchAsync(profileId);
                if (!created.IsSuccess || !created.HasValue)
                {
                    SetUiMessage(MapFaultOrServerKey(created.FaultCode, created.ServerMessage, KEY_MATCH_CREATE_FAILED));
                    return;
                }

                if (created.Value.MatchId <= INVALID_ID)
                {
                    string createUiKey = MatchBusinessErrorMapper.MsapCreateBusinessCodeToUiKey(created.Value.Code);
                    SetUiMessage(createUiKey);
                    return;
                }

                string createdCode = (created.Value.Code ?? EMPTY).Trim();
                if (string.IsNullOrWhiteSpace(createdCode))
                {
                    SetUiMessage(KEY_MATCH_CREATE_FAILED);
                    return;
                }

                var subscribe = await matchHub.SubscribeLobbyAsync(created.Value.MatchId, userId);
                if (!subscribe.IsSuccess || !subscribe.HasValue || !subscribe.Value.Success)
                {
                    SetUiMessage(MapFaultOrServerKey(subscribe.FaultCode, subscribe.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                LobbyRequested?.Invoke(BuildLobbyVmFromCreate(created.Value, createdCode));
            }
            catch (Exception ex)
            {
                Logger.Error("CreateOrJoinViewModel.CreateMatchAsync", ex);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task JoinMatchAsync()
        {
            ClearUiMessage();
            ValidateJoinInputs();

            if (HasErrors)
            {
                SetUiMessage(KEY_MATCH_CODE_REQUIRED);
                return;
            }

            try
            {
                IsBusy = true;

                var connect = await matchHub.ConnectAsync();
                if (!connect.IsSuccess)
                {
                    SetUiMessage(MapFaultOrServerKey(connect.FaultCode, connect.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                string safeCode = (MatchCodeInput ?? EMPTY).Trim();

                var joined = await matchHub.JoinMatchAsync(safeCode, userId);

                if (!joined.IsSuccess || !joined.HasValue)
                {
                    SetUiMessage(MapFaultOrServerKey(joined.FaultCode, joined.ServerMessage, KEY_MATCH_JOIN_FAILED));
                    return;
                }

                if (joined.Value.MatchId <= INVALID_ID)
                {
                    string joinUiKey = MatchBusinessErrorMapper.MapJoinBusinessCodeToUiKey(joined.Value.Code);
                    SetUiMessage(joinUiKey);
                    return;
                }

                var subscribe = await matchHub.SubscribeLobbyAsync(joined.Value.MatchId, userId);
                if (!subscribe.IsSuccess || !subscribe.HasValue || !subscribe.Value.Success)
                {
                    SetUiMessage(MapFaultOrServerKey(subscribe.FaultCode, subscribe.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                LobbyRequested?.Invoke(BuildLobbyVmFromJoin(joined.Value));
            }
            catch (Exception ex)
            {
                Logger.Error("CreateOrJoinViewModel.JoinMatchAsync", ex);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private GameLobbyViewModel BuildLobbyVmFromJoin(GuessWhoCore.Contracts.Requests.JoinMatchResponse response)
        {
            return new GameLobbyViewModel(
                matchHub,
                avatarPathResolver,
                uiFaultMapper,
                localize,
                matchId: response.MatchId,
                matchCode: response.Code ?? EMPTY,
                currentUserId: userId,
                hostUserId: response.HostUserId,
                initialVisibility: response.Visibility,
                initialMode: response.Mode,
                initialPlayers: response.Players ?? new List<LobbyPlayerDto>());
        }

        private GameLobbyViewModel BuildLobbyVmFromCreate(CreateMatchResponse created, string createdCode)
        {
            LobbyPlayerDto hostPlayer = BuildHostPlayerDto(created.MatchId);

            var initialPlayers = new List<LobbyPlayerDto> { hostPlayer };

            return new GameLobbyViewModel(
                matchHub,
                avatarPathResolver,
                uiFaultMapper,
                localize,
                matchId: created.MatchId,
                matchCode: createdCode,
                currentUserId: userId,
                hostUserId: userId,
                initialVisibility: created.VisibilityId != 0 ? created.VisibilityId : DEFAULT_VISIBILITY_PRIVATE,
                initialMode: created.ModeId != 0 ? created.ModeId : DEFAULT_MODE_CLASSIC,
                initialPlayers: initialPlayers);
        }

        private LobbyPlayerDto BuildHostPlayerDto(long matchId)
        {
            string displayName = ReadSessionString(sessionContext, SESSION_PROP_DISPLAY_NAME);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = userId.ToString();
            }

            string avatarId =
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR_ID) ??
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR_KEY) ??
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR) ??
                EMPTY;

            return new LobbyPlayerDto
            {
                MatchId = matchId,
                UserId = userId,
                DisplayName = displayName,
                AvatarId = avatarId,
                SlotNumber = HOST_SLOT_NUMBER,
                IsReady = true,
                IsHost = true
            };
        }

        private static string ReadSessionString(SessionContext session, string propertyName)
        {
            if (session == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return EMPTY;
            }

            PropertyInfo prop = session.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (prop == null)
            {
                return EMPTY;
            }

            object value = prop.GetValue(session, null);
            return value is string s ? (s ?? EMPTY) : EMPTY;
        }

        private void ValidateJoinInputs()
        {
            string safeCode = (MatchCodeInput ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeCode))
            {
                dataErrors.ReplaceAllErrors(new Dictionary<string, IReadOnlyList<string>>
                {
                    { PROP_MATCH_CODE_INPUT, new[] { localize(KEY_MATCH_CODE_REQUIRED) } }
                });

                return;
            }

            dataErrors.ClearAllErrors();
        }

        private string MapFaultOrServerKey(string faultCode, string serverMessageKey, string fallbackKey)
        {
            UiKeyMapping mapping = uiFaultMapper.Map(faultCode);

            if (mapping.IsMapped)
            {
                return mapping.UiKey;
            }

            if (!string.IsNullOrWhiteSpace(serverMessageKey))
            {
                return serverMessageKey;
            }

            return fallbackKey;
        }

        private void SetUiMessage(string messageKey)
        {
            UiMessage = localize(messageKey ?? KEY_UI_GENERIC_ERROR);
        }

        private void ClearUiMessage() => UiMessage = EMPTY;

        private void RaiseCommandStates()
        {
            if (CreateMatchCommand is AsyncRelayCommand create)
            {
                create.RaiseCanExecuteChanged();
            }

            if (JoinMatchCommand is AsyncRelayCommand join)
            {
                join.RaiseCanExecuteChanged();
            }

            if (BackCommand is RelayCommand back)
            {
                back.RaiseCanExecuteChanged();
            }
        }
    }
}
