using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
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
        private const string KEY_UI_ERROR_TITLE = "FriendErrorTitle";
        private const string KEY_UI_UNEXPECTED_ERROR = "UiGenericError";

        private readonly IFriendAppService friendAppService;
        private readonly IAlertService alertService;
        private readonly IUiFaultMapper friendFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;

        private ObservableCollection<UserProfileSearchResult> friends;
        private ObservableCollection<UserProfileSearchResult> searchResults;
        private UserProfileSearchResult selectedProfile;
        private string searchText;

        public FriendViewModel(
            IFriendAppService friendAppService,
            IAlertService alertService,
            IUiFaultMapper friendFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext)
        {
            this.friendAppService = friendAppService ?? throw new ArgumentNullException(nameof(friendAppService));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.friendFaultMapper = friendFaultMapper ?? throw new ArgumentNullException(nameof(friendFaultMapper));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));

            Friends = new ObservableCollection<UserProfileSearchResult>();
            SearchResults = new ObservableCollection<UserProfileSearchResult>();

            LoadFriendsCommand = new AsyncRelayCommand(LoadFriendsAsync, () => !IsBusy);
            SearchProfilesCommand = new AsyncRelayCommand(SearchProfilesAsync, () => !IsBusy);
            SendFriendRequestCommand = new AsyncRelayCommand(SendFriendRequestAsync, () => !IsBusy && SelectedProfile != null);
        }

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }
        public UserProfileSearchResult SelectedProfile { get => selectedProfile; set { if (SetProperty(ref selectedProfile, value)) SendFriendRequestCommand.RaiseCanExecuteChanged(); } }
        public ObservableCollection<UserProfileSearchResult> Friends { get => friends; set => SetProperty(ref friends, value); }
        public ObservableCollection<UserProfileSearchResult> SearchResults { get => searchResults; set => SetProperty(ref searchResults, value); }

        public AsyncRelayCommand LoadFriendsCommand { get; }
        public AsyncRelayCommand SearchProfilesCommand { get; }
        public AsyncRelayCommand SendFriendRequestCommand { get; }

        private async Task LoadFriendsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.GetFriendsAsync(new GetFriendsRequest { AccountId = sessionContext.UserId.ToString() });
                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }
                if (result.Value?.Friends != null) Friends = new ObservableCollection<UserProfileSearchResult>(result.Value.Friends);
            }
            catch (Exception ex) { Logger.Error("LoadFriends", ex); ShowUnexpectedError(); }
            finally { IsBusy = false; }
        }

        private async Task SearchProfilesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.SearchProfilesAsync(new SearchProfileRequest { DisplayName = SearchText });
                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }
                if (result.Value?.Profiles != null) SearchResults = new ObservableCollection<UserProfileSearchResult>(result.Value.Profiles);
            }
            catch (Exception ex) { Logger.Error("Search", ex); ShowUnexpectedError(); }
            finally { IsBusy = false; }
        }

        private async Task SendFriendRequestAsync()
        {
            IsBusy = true;
            try
            {
                var result = await friendAppService.SendFriendRequestAsync(new SendFriendRequestRequest { FromAccountId = sessionContext.UserId, ToUserId = SelectedProfile.UserId });
                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }
                alertService.Info(localizationService.Get("FriendRequestSent"), localizationService.Get("SuccessTitle"));
            }
            catch (Exception ex) { Logger.Error("SendRequest", ex); ShowUnexpectedError(); }
            finally { IsBusy = false; }
        }

        private void ShowUnexpectedError() => alertService.Error(localizationService.Get(KEY_UI_UNEXPECTED_ERROR), localizationService.Get(KEY_UI_ERROR_TITLE));

        private void ShowFriendError(string faultCode)
        {
            if (friendFaultMapper.TryMap(faultCode, out string uiKey))
            {
                alertService.Error(localizationService.Get(uiKey), localizationService.Get(KEY_UI_ERROR_TITLE));
            }
            else
            {
                ShowUnexpectedError();
            }
        }
    }
}