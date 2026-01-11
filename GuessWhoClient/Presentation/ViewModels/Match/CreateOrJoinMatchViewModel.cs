using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Presentation.ViewsModels.Base;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        private const string KEY_MATCH_CREATE_FAILED = "UiMatchCreateFailed";      
        private const string KEY_MATCH_JOIN_FAILED = "UiMatchJoinFailed";          
        private const string KEY_MATCH_CODE_REQUIRED = "UiMatchCodeRequired";      
        private const string KEY_MATCH_INVALID_ARGS = "UiMatchCreateInvalidArgs";  

        private const string CONTEXT_CREATE = "CreateOrJoinViewModel.CreateMatchAsync";
        private const string CONTEXT_JOIN = "CreateOrJoinViewModel.JoinMatchAsync";

        private readonly IMatchClient matchClient;
        private readonly IMatchUiKeyResolver uiKeyResolver;
        private readonly IGameLobbyViewModelFactory lobbyViewModelFactory;
        private readonly Func<string, string> localize;

        private readonly long profileId;
        private readonly long userId;

        private readonly DataErrors dataErrors = new DataErrors();

        private string matchCodeInput;
        private string uiMessage;

        public CreateOrJoinViewModel(
            IMatchClient matchClient,
            long profileId,
            long userId,
            IMatchUiKeyResolver uiKeyResolver,
            IGameLobbyViewModelFactory lobbyViewModelFactory,
            Func<string, string> localize)
        {
            this.matchClient = matchClient ?? 
                throw new ArgumentNullException(nameof(matchClient));
            this.uiKeyResolver = uiKeyResolver ?? 
                throw new ArgumentNullException(nameof(uiKeyResolver));
            this.lobbyViewModelFactory = lobbyViewModelFactory ?? 
                throw new ArgumentNullException(nameof(lobbyViewModelFactory));
            this.localize = localize ?? 
                throw new ArgumentNullException(nameof(localize));

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

            await ExecuteMatchOperationAsync(async () =>
            {
                var connect = await matchClient.ConnectAsync();
                if (!connect.IsSuccess)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        connect.FaultCode, connect.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                var created = await matchClient.CreateMatchAsync(profileId);
                if (!created.IsSuccess || !created.HasValue)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        created.FaultCode, created.ServerMessage, KEY_MATCH_CREATE_FAILED));
                    return;
                }

                if (created.Value.MatchId <= INVALID_ID)
                {
                    SetUiMessage(uiKeyResolver.ResolveCodeOrFallback(
                        created.Value.Code, KEY_MATCH_CREATE_FAILED));
                    return;
                }

                string createdCode = (created.Value.Code ?? EMPTY).Trim();
                if (string.IsNullOrWhiteSpace(createdCode))
                {
                    SetUiMessage(KEY_MATCH_CREATE_FAILED);
                    return;
                }

                var subscribe = await matchClient.SubscribeLobbyAsync(created.Value.MatchId, userId);
                if (!subscribe.IsSuccess || !subscribe.HasValue || !subscribe.Value.Success)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        subscribe.FaultCode, subscribe.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                GameLobbyViewModel lobbyVm = lobbyViewModelFactory.CreateFromCreate(created.Value, createdCode, userId);
                LobbyRequested?.Invoke(lobbyVm);
            }, CONTEXT_CREATE);
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

            await ExecuteMatchOperationAsync(async () =>
            {
                var connect = await matchClient.ConnectAsync();

                if (!connect.IsSuccess)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        connect.FaultCode, connect.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                string safeCode = (MatchCodeInput ?? EMPTY).Trim();

                var joined = await matchClient.JoinMatchAsync(safeCode, userId);

                if (!joined.IsSuccess || !joined.HasValue)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        joined.FaultCode, joined.ServerMessage, KEY_MATCH_JOIN_FAILED));
                    return;
                }

                if (joined.Value.MatchId <= INVALID_ID)
                {
                    SetUiMessage(uiKeyResolver.ResolveCodeOrFallback(
                        joined.Value.Code, KEY_MATCH_JOIN_FAILED));
                    return;
                }

                var subscribe = await matchClient.SubscribeLobbyAsync(joined.Value.MatchId, userId);

                if (!subscribe.IsSuccess || !subscribe.HasValue || !subscribe.Value.Success)
                {
                    SetUiMessage(uiKeyResolver.ResolveFaultOrServerKey(
                        subscribe.FaultCode, subscribe.ServerMessage, KEY_UI_GENERIC_ERROR));
                    return;
                }

                GameLobbyViewModel lobbyViewModel = lobbyViewModelFactory.CreateFromJoin(joined.Value, userId);
                LobbyRequested?.Invoke(lobbyViewModel);
            }, CONTEXT_JOIN);
        }

        private async Task ExecuteMatchOperationAsync(Func<Task> operationAsync, string logContext)
        {
            if (operationAsync == null)
            {
                throw new ArgumentNullException(nameof(operationAsync));
            }

            if (string.IsNullOrWhiteSpace(logContext))
            {
                throw new ArgumentException("logContext is required.", nameof(logContext));
            }

            try
            {
                IsBusy = true;
                await operationAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(logContext, ex);
                SetUiMessage(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
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
