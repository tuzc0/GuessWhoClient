using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoClient.Application.ErrorHandling.Friends;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Friends
{
    public sealed class FriendViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FriendViewModel));
        private const string LOG_CTX_LOAD = "FriendViewModel.LoadFriends.Unexpected";
        private const string LOG_CTX_SEARCH = "FriendViewModel.SearchProfiles.Unexpected";
        private const string LOG_CTX_OPERATION = "FriendViewModel.FriendOperation.Unexpected";
        private const string KEY_UI_ERROR_TITLE = "FriendErrorTitle";
        private const string KEY_UI_SUCCESS_TITLE = "SuccessTitle";
        private const string KEY_GENERIC_ERROR = "UiGenericError";

        private readonly IFriendAppService friendAppService;
        private readonly IAlertService alertService;
        private readonly FriendUiFaultMapper friendFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;

        private ObservableCollection<UserProfileSearchResult> friends;
        private ObservableCollection<UserProfileSearchResult> searchResults;
        private ObservableCollection<FriendRequest> pendingRequests;
        private UserProfileSearchResult selectedProfile;
        private FriendRequest selectedPendingRequest;
        private string searchText;

        public FriendViewModel(
            IFriendAppService friendAppService,
            IAlertService alertService,
            FriendUiFaultMapper friendFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager)
        {
            this.friendAppService = friendAppService ?? throw new ArgumentNullException(nameof(friendAppService));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.friendFaultMapper = friendFaultMapper ?? throw new ArgumentNullException(nameof(friendFaultMapper));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ?? throw new ArgumentNullException(nameof(gameScreenManager));

            Friends = new ObservableCollection<UserProfileSearchResult>();
            SearchResults = new ObservableCollection<UserProfileSearchResult>();
            PendingRequests = new ObservableCollection<FriendRequest>();

            LoadFriendsCommand = new AsyncRelayCommand(LoadFriendsAsync, CanExecuteCommands);
            SearchProfilesCommand = new AsyncRelayCommand(SearchProfilesAsync, CanExecuteCommands);
            SendFriendRequestCommand = new AsyncRelayCommand(SendFriendRequestAsync, CanExecuteSendRequest);
            LoadPendingRequestsCommand = new AsyncRelayCommand(LoadPendingRequestsAsync, CanExecuteCommands);
            AcceptFriendRequestCommand = new AsyncRelayCommand(AcceptFriendRequestAsync, CanExecutePendingAction);
            RejectFriendRequestCommand = new AsyncRelayCommand(RejectFriendRequestAsync, CanExecutePendingAction);
        }

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }
        public UserProfileSearchResult SelectedProfile { get => selectedProfile; set { if (SetProperty(ref selectedProfile, value)) SendFriendRequestCommand.RaiseCanExecuteChanged(); } }
        public FriendRequest SelectedPendingRequest { get => selectedPendingRequest; set { if (SetProperty(ref selectedPendingRequest, value)) { AcceptFriendRequestCommand.RaiseCanExecuteChanged(); RejectFriendRequestCommand.RaiseCanExecuteChanged(); } } }
        public ObservableCollection<UserProfileSearchResult> Friends { get => friends; set => SetProperty(ref friends, value); }
        public ObservableCollection<UserProfileSearchResult> SearchResults { get => searchResults; set => SetProperty(ref searchResults, value); }
        public ObservableCollection<FriendRequest> PendingRequests { get => pendingRequests; set => SetProperty(ref pendingRequests, value); }

        public AsyncRelayCommand LoadFriendsCommand { get; }
        public AsyncRelayCommand SearchProfilesCommand { get; }
        public AsyncRelayCommand SendFriendRequestCommand { get; }
        public AsyncRelayCommand LoadPendingRequestsCommand { get; }
        public AsyncRelayCommand AcceptFriendRequestCommand { get; }
        public AsyncRelayCommand RejectFriendRequestCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoadFriendsCommand.RaiseCanExecuteChanged();
            SearchProfilesCommand.RaiseCanExecuteChanged();
            SendFriendRequestCommand.RaiseCanExecuteChanged();
            LoadPendingRequestsCommand.RaiseCanExecuteChanged();
            AcceptFriendRequestCommand.RaiseCanExecuteChanged();
            RejectFriendRequestCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands() => !IsBusy;
        private bool CanExecuteSendRequest() => !IsBusy && SelectedProfile != null;
        private bool CanExecutePendingAction() => !IsBusy && SelectedPendingRequest != null;

        public async Task LoadFriendsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.GetFriendsAsync(new GetFriendsRequest { AccountId = sessionContext.UserId.ToString() });
                if (result.IsSuccess && result.Value?.Friends != null) Friends = new ObservableCollection<UserProfileSearchResult>(result.Value.Friends);
            }
            finally { IsBusy = false; }
        }

        public async Task LoadPendingRequestsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.GetPendingRequestsAsync(new GetPendingFriendRequestsRequest { AccountId = sessionContext.UserId.ToString() });
                if (result.IsSuccess && result.Value?.Requests != null) PendingRequests = new ObservableCollection<FriendRequest>(result.Value.Requests);
            }
            finally { IsBusy = false; }
        }

        public async Task AcceptFriendRequestAsync() => await ProcessRequestOperation(friendAppService.AcceptFriendRequestAsync, "FriendRequestAccepted");
        public async Task RejectFriendRequestAsync() => await ProcessRequestOperation(friendAppService.RejectFriendRequestAsync, "FriendRequestRejected");

        private async Task ProcessRequestOperation(Func<FriendRequestOperationRequest, Task<WcfCallResult<BasicResponse>>> operation, string successMessageKey)
        {
            if (SelectedPendingRequest == null) return;
            IsBusy = true;
            try
            {
                var result = await operation(new FriendRequestOperationRequest
                {
                    AccountId = sessionContext.UserId.ToString(),
                    FriendRequestId = SelectedPendingRequest.FriendRequestId.ToString()
                });
                if (result.IsSuccess)
                {
                    alertService.Info(localizationService.Get(successMessageKey), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                    await LoadFriendsAsync();
                    await LoadPendingRequestsAsync();
                }
                else ShowFriendError(result.FaultCode);
            }
            catch (Exception ex) { Logger.Error(LOG_CTX_OPERATION, ex); }
            finally { IsBusy = false; }
        }

        public async Task SearchProfilesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.SearchProfilesAsync(new SearchProfileRequest { DisplayName = SearchText });
                if (result.IsSuccess && result.Value?.Profiles != null) SearchResults = new ObservableCollection<UserProfileSearchResult>(result.Value.Profiles);
            }
            finally { IsBusy = false; }
        }

        public async Task SendFriendRequestAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.SendFriendRequestAsync(new SendFriendRequestRequest { FromAccountId = sessionContext.UserId, ToUserId = SelectedProfile.UserId });
                if (result.IsSuccess) alertService.Info(localizationService.Get("FriendRequestSent"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                else ShowFriendError(result.FaultCode);
            }
            finally { IsBusy = false; }
        }

        private void ShowFriendError(string faultCode)
        {
            var mapping = friendFaultMapper.Map(faultCode);
            alertService.Error(localizationService.Get(mapping.IsMapped ? mapping.UiKey : KEY_GENERIC_ERROR), localizationService.Get(KEY_UI_ERROR_TITLE));
        }
    }
}