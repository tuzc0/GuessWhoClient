using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
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
        private readonly IUiFaultMapper friendFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;

        private ObservableCollection<UserProfileSearchResult> friends;
        private ObservableCollection<UserProfileSearchResult> searchResults;
        private UserProfileSearchResult selectedProfile;
        private string searchText;

        public FriendViewModel(
            IFriendAppService friendAppService,
            IAlertService alertService,
            IUiFaultMapper friendFaultMapper,
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

            LoadFriendsCommand = new AsyncRelayCommand(LoadFriendsAsync, CanExecuteCommands);
            SearchProfilesCommand = new AsyncRelayCommand(SearchProfilesAsync, CanExecuteCommands);
            SendFriendRequestCommand = new AsyncRelayCommand(SendFriendRequestAsync, CanExecuteSendRequest);
        }

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }

        public UserProfileSearchResult SelectedProfile
        {
            get => selectedProfile;
            set
            {
                if (SetProperty(ref selectedProfile, value))
                {
                    SendFriendRequestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<UserProfileSearchResult> Friends { get => friends; set => SetProperty(ref friends, value); }
        public ObservableCollection<UserProfileSearchResult> SearchResults { get => searchResults; set => SetProperty(ref searchResults, value); }

        public AsyncRelayCommand LoadFriendsCommand { get; }
        public AsyncRelayCommand SearchProfilesCommand { get; }
        public AsyncRelayCommand SendFriendRequestCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoadFriendsCommand.RaiseCanExecuteChanged();
            SearchProfilesCommand.RaiseCanExecuteChanged();
            SendFriendRequestCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands() => !IsBusy;
        private bool CanExecuteSendRequest() => !IsBusy && SelectedProfile != null;

        private async Task LoadFriendsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.GetFriendsAsync(new GetFriendsRequest { AccountId = sessionContext.UserId.ToString() });

                if (!result.IsSuccess)
                {
                    ShowFriendError(result.FaultCode);
                    return;
                }

                if (result.Value?.Friends != null)
                {
                    Friends = new ObservableCollection<UserProfileSearchResult>(result.Value.Friends);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOAD, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private async Task SearchProfilesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.SearchProfilesAsync(new SearchProfileRequest { DisplayName = SearchText });

                if (!result.IsSuccess)
                {
                    ShowFriendError(result.FaultCode);
                    return;
                }

                if (result.Value?.Profiles != null)
                {
                    SearchResults = new ObservableCollection<UserProfileSearchResult>(result.Value.Profiles);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_SEARCH, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private async Task SendFriendRequestAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.SendFriendRequestAsync(new SendFriendRequestRequest
                {
                    FromAccountId = sessionContext.UserId,
                    ToUserId = SelectedProfile.UserId
                });

                if (!result.IsSuccess)
                {
                    ShowFriendError(result.FaultCode);
                    return;
                }

                alertService.Info(localizationService.Get("FriendRequestSent"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_OPERATION, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private void ShowUiError(string messageKey)
        {
            alertService.Error(localizationService.Get(messageKey), localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private void ShowFriendError(string faultCode)
        {
            var mapping = friendFaultMapper.Map(faultCode);
            string key = mapping.IsMapped ? mapping.UiKey : KEY_GENERIC_ERROR;
            alertService.Error(localizationService.Get(key), localizationService.Get(KEY_UI_ERROR_TITLE));
        }
    }
}