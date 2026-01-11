using GuessWhoClient.Application.Services.Friends;
using GuessWhoClient.Assets;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Base;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly IAvatarPathResolver avatarPathResolver;

        private ObservableCollection<UserProfileSearchResult> friends;
        private ObservableCollection<UserProfileSearchResult> searchResults;
        private ObservableCollection<UserProfileSearchResult> receivedRequests;
        private UserProfileSearchResult selectedProfile;
        private UserProfileSearchResult selectedReceivedRequest;
        private string searchText;

        public FriendViewModel(
            IFriendAppService friendAppService,
            IAlertService alertService,
            IUiFaultMapper friendFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager,
            IAvatarPathResolver avatarPathResolver)
        {
            this.friendAppService = friendAppService ?? throw new ArgumentNullException(nameof(friendAppService));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.friendFaultMapper = friendFaultMapper ?? throw new ArgumentNullException(nameof(friendFaultMapper));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ?? throw new ArgumentNullException(nameof(gameScreenManager));
            this.avatarPathResolver = avatarPathResolver ?? throw new ArgumentNullException(nameof(avatarPathResolver));

            Friends = new ObservableCollection<UserProfileSearchResult>();
            SearchResults = new ObservableCollection<UserProfileSearchResult>();
            ReceivedRequests = new ObservableCollection<UserProfileSearchResult>();

            LoadFriendsCommand = new AsyncRelayCommand(LoadFriendsAsync, CanExecuteCommands);
            SearchProfilesCommand = new AsyncRelayCommand(SearchProfilesAsync, CanExecuteCommands);
            SendFriendRequestCommand = new AsyncRelayCommand(SendFriendRequestAsync, CanExecuteSendRequest);
            AcceptFriendRequestCommand = new AsyncRelayCommand(AcceptFriendRequestAsync, CanExecuteAcceptRequest);
            RejectFriendRequestCommand = new AsyncRelayCommand(RejectFriendRequestAsync, CanExecuteAcceptRequest);
            BackCommand = new RelayCommand(OnBackRequested);
        }

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }

        public UserProfileSearchResult SelectedProfile
        {
            get => selectedProfile;
            set { if (SetProperty(ref selectedProfile, value)) SendFriendRequestCommand.RaiseCanExecuteChanged(); }
        }

        public UserProfileSearchResult SelectedReceivedRequest
        {
            get => selectedReceivedRequest;
            set
            {
                if (SetProperty(ref selectedReceivedRequest, value))
                {
                    AcceptFriendRequestCommand.RaiseCanExecuteChanged();
                    RejectFriendRequestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<UserProfileSearchResult> Friends { get => friends; set => SetProperty(ref friends, value); }
        public ObservableCollection<UserProfileSearchResult> SearchResults { get => searchResults; set => SetProperty(ref searchResults, value); }
        public ObservableCollection<UserProfileSearchResult> ReceivedRequests { get => receivedRequests; set => SetProperty(ref receivedRequests, value); }

        public AsyncRelayCommand LoadFriendsCommand { get; }
        public AsyncRelayCommand SearchProfilesCommand { get; }
        public AsyncRelayCommand SendFriendRequestCommand { get; }
        public AsyncRelayCommand AcceptFriendRequestCommand { get; }
        public AsyncRelayCommand RejectFriendRequestCommand { get; }
        public RelayCommand BackCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoadFriendsCommand.RaiseCanExecuteChanged();
            SearchProfilesCommand.RaiseCanExecuteChanged();
            SendFriendRequestCommand.RaiseCanExecuteChanged();
            AcceptFriendRequestCommand.RaiseCanExecuteChanged();
            RejectFriendRequestCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands() => !IsBusy;
        private bool CanExecuteSendRequest() => !IsBusy && SelectedProfile != null;
        private bool CanExecuteAcceptRequest() => !IsBusy && SelectedReceivedRequest != null;

        private void OnBackRequested() => gameScreenManager.ShowScreen(GameScreenType.MainMenu);

        public async Task LoadFriendsAsync()
        {
            IsBusy = true;
            try
            {
                long currentAccountId = sessionContext.UserId;

                // Cargar Amigos
                var friendsResult = await friendAppService.GetFriendsAsync(new GetFriendsRequest { AccountId = currentAccountId });
                if (friendsResult.IsSuccess && friendsResult.Value?.Friends != null)
                {
                    var updatedFriends = friendsResult.Value.Friends.ToList();
                    foreach (var f in updatedFriends) f.AvatarId = avatarPathResolver.Resolve(f.AvatarId);
                    Friends = new ObservableCollection<UserProfileSearchResult>(updatedFriends);
                }

                // Cargar Solicitudes Pendientes
                var pendingResult = await friendAppService.GetPendingRequestsAsync(new GetPendingFriendRequestsRequest { AccountId = currentAccountId });
                if (pendingResult.IsSuccess && pendingResult.Value?.Requests != null)
                {
                    var requestsList = new ObservableCollection<UserProfileSearchResult>();
                    foreach (var r in pendingResult.Value.Requests)
                    {
                        // Mapeamos FriendRequestId al campo UserId para que la UI pueda enviarlo de vuelta al aceptar/rechazar
                        requestsList.Add(new UserProfileSearchResult
                        {
                            UserId = r.FriendRequestId,
                            DisplayName = r.RequesterDisplayName,
                            AvatarId = avatarPathResolver.Resolve(string.Empty)
                        });
                    }
                    ReceivedRequests = requestsList;
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
                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }
                if (result.Value?.Profiles != null)
                {
                    var profiles = result.Value.Profiles.ToList();
                    foreach (var p in profiles) p.AvatarId = avatarPathResolver.Resolve(p.AvatarId);
                    SearchResults = new ObservableCollection<UserProfileSearchResult>(profiles);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_SEARCH, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
                SearchProfilesCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task SendFriendRequestAsync()
        {
            if (SelectedProfile == null) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.SendFriendRequestAsync(new SendFriendRequestRequest
                {
                    FromAccountId = sessionContext.UserId,
                    ToUserId = SelectedProfile.UserId
                });

                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }

                alertService.Info(localizationService.Get("UiFriendsRequestSent"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                SelectedProfile = null;
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_OPERATION, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private async Task AcceptFriendRequestAsync()
        {
            if (SelectedReceivedRequest == null) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.AcceptFriendRequestAsync(new FriendRequestOperationRequest
                {
                    AccountId = sessionContext.UserId,
                    FriendRequestId = SelectedReceivedRequest.UserId // Aquí va el ID de la solicitud
                });

                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }

                alertService.Info(localizationService.Get("UiFriendsRequestAccepted"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                await LoadFriendsAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_OPERATION, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private async Task RejectFriendRequestAsync()
        {
            if (SelectedReceivedRequest == null) return;
            IsBusy = true;
            try
            {
                var result = await friendAppService.RejectFriendRequestAsync(new FriendRequestOperationRequest
                {
                    AccountId = sessionContext.UserId,
                    FriendRequestId = SelectedReceivedRequest.UserId
                });

                if (!result.IsSuccess) { ShowFriendError(result.FaultCode); return; }

                await LoadFriendsAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_OPERATION, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private void ShowUiError(string messageKey) => alertService.Error(localizationService.Get(messageKey), localizationService.Get(KEY_UI_ERROR_TITLE));

        private void ShowFriendError(string faultCode)
        {
            var mapping = friendFaultMapper.Map(faultCode);
            string key = mapping.IsMapped ? mapping.UiKey : KEY_GENERIC_ERROR;
            alertService.Error(localizationService.Get(key), localizationService.Get(KEY_UI_ERROR_TITLE));
        }
    }
}