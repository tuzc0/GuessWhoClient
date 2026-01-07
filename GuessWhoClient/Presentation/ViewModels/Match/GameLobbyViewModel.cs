using GuessWhoClient.Assets;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Presentation.ViewModels.Base;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class GameLobbyViewModel : ViewModelBase, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameLobbyViewModel));

        private const string EMPTY = "";
        private const long INVALID_ID = 0;

        private const byte VIS_PRIVATE = 2;

        private const byte MODE_CLASSIC = 1;
        private const byte MODE_TOURNAMENT = 2;

        private const int MIN_PLAYERS_TO_START = 2;

        private const string KEY_UI_GENERIC_ERROR = "UiGenericError";
        private const string KEY_MATCH_PUBLIC_NOT_SUPPORTED = "Match.PublicNotSupported";
        private const string KEY_MATCH_TOURNAMENT_NOT_AVAILABLE = "Match.TournamentNotAvailable";
        private const string KEY_MATCH_SET_PRIVATE_FAILED = "Match.SetPrivateFailed";
        private const string KEY_MATCH_READY_FAILED = "Match.ReadyFailed";

        private readonly MatchHub matchHub;
        private readonly IAvatarPathResolver avatarPathResolver;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly Func<string, string> localize;

        private readonly long matchId;
        private readonly long currentUserId;
        private readonly long hostUserId;

        private bool isPrivate;
        private bool isApplyingPrivacy;
        private bool hasHubEventsAttached;

        private string uiMessage;
        private string selectedBoard;

        public GameLobbyViewModel(
            MatchHub matchHub,
            IAvatarPathResolver avatarPathResolver,
            IUiFaultMapper uiFaultMapper,
            Func<string, string> localize,
            long matchId,
            string matchCode,
            long currentUserId,
            long hostUserId,
            byte initialVisibility,
            byte initialMode,
            IReadOnlyList<LobbyPlayerDto> initialPlayers)
        {
            this.matchHub = matchHub ?? throw new ArgumentNullException(nameof(matchHub));
            this.avatarPathResolver = avatarPathResolver ?? throw new ArgumentNullException(nameof(avatarPathResolver));
            this.uiFaultMapper = uiFaultMapper ?? throw new ArgumentNullException(nameof(uiFaultMapper));
            this.localize = localize ?? throw new ArgumentNullException(nameof(localize));

            this.matchId = matchId;
            this.currentUserId = currentUserId;
            this.hostUserId = hostUserId;

            LobbyTitle = "Lobby";
            MatchCode = (matchCode ?? EMPTY).Trim();

            LobbyPlayers = new ObservableCollection<LobbyPlayerItemViewModel>();
            GameModes = new ObservableCollection<GameModeItemViewModel>();
            Boards = new ObservableCollection<string>();

            BuildGameModes(initialMode);
            BuildBoards();

            isPrivate = initialVisibility == VIS_PRIVATE;

            SelectModeCommand = new RelayCommandWithParam<GameModeItemViewModel>(SelectMode, CanSelectMode);
            ToggleReadyCommand = new AsyncRelayCommand(ToggleReadyAsync, CanToggleReady);
            StartCommand = new AsyncRelayCommand(StartAsync, CanStartAsync);
            LeaveLobbyCommand = new AsyncRelayCommand(LeaveLobbyAsync, () => !IsBusy);

            LoadInitialPlayers(initialPlayers ?? Array.Empty<LobbyPlayerDto>());

            AttachHubEvents();
        }

        public event Action ExitRequested;

        public string LobbyTitle { get; }
        public string MatchCode { get; }

        public ObservableCollection<LobbyPlayerItemViewModel> LobbyPlayers { get; }
        public ObservableCollection<GameModeItemViewModel> GameModes { get; }
        public ObservableCollection<string> Boards { get; }

        public ICommand SelectModeCommand { get; }
        public ICommand ToggleReadyCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand LeaveLobbyCommand { get; }

        public bool IsCurrentUserHost => currentUserId == hostUserId;

        public bool IsPrivate
        {
            get => isPrivate;
            set
            {
                if (!IsCurrentUserHost || isApplyingPrivacy)
                {
                    return;
                }

                if (!SetProperty(ref isPrivate, value))
                {
                    return;
                }

                _ = ApplyPrivacyAsync(value);
            }
        }

        public string SelectedBoard
        {
            get => selectedBoard;
            set => SetProperty(ref selectedBoard, value ?? EMPTY);
        }

        public bool IsCurrentPlayerReady
        {
            get
            {
                LobbyPlayerItemViewModel player = FindPlayerVm(currentUserId);
                return player != null && player.IsReady;
            }
        }

        public bool CanStart => IsCurrentUserHost && AreStartConditionsMet();

        public string UiMessage
        {
            get => uiMessage;
            private set => SetProperty(ref uiMessage, value ?? EMPTY);
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            RaiseCommandStates();
        }

        private void BuildBoards()
        {
            Boards.Clear();
            Boards.Add("Default");
            SelectedBoard = Boards.Count > 0 ? Boards[0] : EMPTY;
        }

        private void BuildGameModes(byte initialMode)
        {
            GameModes.Clear();

            var classic = new GameModeItemViewModel(MODE_CLASSIC, "Classic")
            {
                IsSelected = initialMode == MODE_CLASSIC,
                IsEnabled = IsCurrentUserHost
            };

            var tournament = new GameModeItemViewModel(MODE_TOURNAMENT, "Tournament")
            {
                IsSelected = initialMode == MODE_TOURNAMENT,
                IsEnabled = false
            };

            if (!classic.IsSelected && !tournament.IsSelected)
            {
                classic.IsSelected = true;
            }

            GameModes.Add(classic);
            GameModes.Add(tournament);
        }

        private void LoadInitialPlayers(IReadOnlyList<LobbyPlayerDto> players)
        {
            LobbyPlayers.Clear();

            if (players == null || players.Count == 0)
            {
                RaiseLobbyComputedProps();
                return;
            }

            for (int index = 0; index < players.Count; index++)
            {
                UpsertPlayer(players[index]);
            }

            RaiseLobbyComputedProps();
        }

        private bool CanSelectMode(GameModeItemViewModel mode)
        {
            return !IsBusy && IsCurrentUserHost && mode != null && mode.IsEnabled;
        }

        private void SelectMode(GameModeItemViewModel selected)
        {
            ClearUiMessage();

            if (selected == null)
            {
                return;
            }

            if (selected.Id == MODE_TOURNAMENT)
            {
                SetUiMessage(KEY_MATCH_TOURNAMENT_NOT_AVAILABLE);
                return;
            }

            for (int index = 0; index < GameModes.Count; index++)
            {
                GameModes[index].IsSelected = GameModes[index].Id == selected.Id;
            }
        }

        private async Task ApplyPrivacyAsync(bool makePrivate)
        {
            ClearUiMessage();

            if (!makePrivate)
            {
                isApplyingPrivacy = true;
                try
                {
                    isPrivate = true;
                    OnPropertyChanged(nameof(IsPrivate));
                }
                finally
                {
                    isApplyingPrivacy = false;
                }

                SetUiMessage(KEY_MATCH_PUBLIC_NOT_SUPPORTED);
                return;
            }

            try
            {
                IsBusy = true;

                var result = await matchHub.SetMatchPrivateAsync(matchId, currentUserId);

                if (!result.IsSuccess || !result.HasValue || !result.Value.Success)
                {
                    SetPrivacyInternal(false);
                    SetUiMessage(MapToUiKey(result.FaultCode, 
                        result.ServerMessage, KEY_MATCH_SET_PRIVATE_FAILED));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GameLobbyViewModel.ApplyPrivacyAsync", ex);
                SetPrivacyInternal(false);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetPrivacyInternal(bool value)
        {
            isApplyingPrivacy = true;
            try
            {
                isPrivate = value;
                OnPropertyChanged(nameof(IsPrivate));
            }
            finally
            {
                isApplyingPrivacy = false;
            }
        }

        private bool CanToggleReady()
        {
            return !IsBusy && !IsCurrentUserHost;
        }

        private async Task ToggleReadyAsync()
        {
            ClearUiMessage();

            try
            {
                IsBusy = true;

                var result = await matchHub.SetPlayerReadyStatusAsync(matchId, currentUserId);

                if (!result.IsSuccess || !result.HasValue || !result.Value.Success)
                {
                    SetUiMessage(MapToUiKey(result.FaultCode, result.ServerMessage, KEY_MATCH_READY_FAILED));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GameLobbyViewModel.ToggleReadyAsync", ex);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanStartAsync()
        {
            return !IsBusy && CanStart;
        }

        private async Task StartAsync()
        {
            // Cuando tengas StartMatch en server, lo conectamos aquí.
            await Task.CompletedTask;
        }

        private async Task LeaveLobbyAsync()
        {
            ClearUiMessage();

            try
            {
                IsBusy = true;

                await matchHub.UnsubscribeLobbyAsync(matchId, currentUserId);
                await matchHub.LeaveMatchAsync(matchId, currentUserId);

                ExitRequested?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error("GameLobbyViewModel.LeaveLobbyAsync", ex);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AttachHubEvents()
        {
            if (hasHubEventsAttached)
            {
                return;
            }

            matchHub.PlayerJoined += OnPlayerJoined;
            matchHub.PlayerLeft += OnPlayerLeft;
            matchHub.ReadyChanged += OnReadyChanged;

            hasHubEventsAttached = true;
        }

        private void DetachHubEvents()
        {
            if (!hasHubEventsAttached)
            {
                return;
            }

            matchHub.PlayerJoined -= OnPlayerJoined;
            matchHub.PlayerLeft -= OnPlayerLeft;
            matchHub.ReadyChanged -= OnReadyChanged;

            hasHubEventsAttached = false;
        }

        private void OnPlayerJoined(LobbyPlayerDto player)
        {
            if (player == null || player.MatchId != matchId)
            {
                return;
            }

            UpsertPlayer(player);
            RaiseLobbyComputedProps();
        }

        private void OnPlayerLeft(LobbyPlayerDto player)
        {
            if (player == null || player.MatchId != matchId)
            {
                return;
            }

            int index = FindPlayerIndex(player.UserId);

            if (index >= 0)
            {
                LobbyPlayers.RemoveAt(index);
                RaiseLobbyComputedProps();
            }
        }

        private void OnReadyChanged(LobbyPlayerDto player)
        {
            if (player == null || player.MatchId != matchId)
            {
                return;
            }

            UpsertPlayer(player);
            RaiseLobbyComputedProps();
        }

        private void UpsertPlayer(LobbyPlayerDto player)
        {
            if (player == null || player.UserId <= INVALID_ID)
            {
                return;
            }

            int index = FindPlayerIndex(player.UserId);

            if (index < 0)
            {
                LobbyPlayers.Add(CreatePlayerVm(player));
                return;
            }

            LobbyPlayerItemViewModel existing = LobbyPlayers[index];
            if (existing == null)
            {
                LobbyPlayers[index] = CreatePlayerVm(player);
                return;
            }

            existing.DisplayName = player.DisplayName ?? EMPTY;
            existing.AvatarPath = avatarPathResolver.Resolve(player.AvatarId);
            existing.IsReady = player.IsReady;
            existing.IsHost = player.IsHost;
        }

        private LobbyPlayerItemViewModel CreatePlayerVm(LobbyPlayerDto player)
        {
            return new LobbyPlayerItemViewModel
            {
                UserId = player.UserId,
                DisplayName = player.DisplayName ?? EMPTY,
                AvatarPath = avatarPathResolver.Resolve(player.AvatarId),
                IsReady = player.IsReady,
                IsHost = player.IsHost
            };
        }

        private int FindPlayerIndex(long userId)
        {
            for (int index = 0; index < LobbyPlayers.Count; index++)
            {
                LobbyPlayerItemViewModel current = LobbyPlayers[index];
                if (current != null && current.UserId == userId)
                {
                    return index;
                }
            }

            return -1;
        }

        private LobbyPlayerItemViewModel FindPlayerVm(long userId)
        {
            int index = FindPlayerIndex(userId);
            return index >= 0 ? LobbyPlayers[index] : null;
        }

        private bool AreStartConditionsMet()
        {
            if (LobbyPlayers.Count < MIN_PLAYERS_TO_START)
            {
                return false;
            }

            for (int index = 0; index < LobbyPlayers.Count; index++)
            {
                LobbyPlayerItemViewModel p = LobbyPlayers[index];
                if (p == null)
                {
                    continue;
                }

                if (!p.IsHost && !p.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        private void RaiseLobbyComputedProps()
        {
            OnPropertyChanged(nameof(IsCurrentPlayerReady));
            OnPropertyChanged(nameof(CanStart));
            RaiseCommandStates();
        }

        private void RaiseCommandStates()
        {
            if (SelectModeCommand is RelayCommandWithParam<GameModeItemViewModel> select)
            {
                select.RaiseCanExecuteChanged();
            }

            if (ToggleReadyCommand is AsyncRelayCommand ready)
            {
                ready.RaiseCanExecuteChanged();
            }

            if (StartCommand is AsyncRelayCommand start)
            {
                start.RaiseCanExecuteChanged();
            }

            if (LeaveLobbyCommand is AsyncRelayCommand leave)
            {
                leave.RaiseCanExecuteChanged();
            }
        }

        private string MapToUiKey(string faultCode, string serverMessageKey, string fallbackKey)
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

        private void ClearUiMessage()
        {
            UiMessage = EMPTY;
        }

        public void Dispose()
        {
            DetachHubEvents();
        }
    }
}
