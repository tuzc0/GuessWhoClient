using GuessWhoClient.Application.Services.Profile;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public sealed class UpdateProfileViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateProfileViewModel));

        private const string KEY_UI_ERROR_TITLE = "ProfileErrorTitle";
        private const string KEY_UI_SUCCESS_TITLE = "SuccessTitle";
        private const string KEY_GENERIC_ERROR = "UiGenericError";

        private readonly IUpdateProfileAppService profileAppService;
        private readonly IAlertService alertService;
        private readonly IUiFaultMapper profileFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;

        private string displayName;
        private string email;
        private string avatarId;
        private string currentPassword;
        private string newPassword;
        private bool isEditing;
        private bool isPasswordModalVisible;

        public UpdateProfileViewModel(
            IUpdateProfileAppService profileAppService,
            IAlertService alertService,
            IUiFaultMapper profileFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager)
        {
            this.profileAppService = profileAppService ?? throw new ArgumentNullException(nameof(profileAppService));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.profileFaultMapper = profileFaultMapper ?? throw new ArgumentNullException(nameof(profileFaultMapper));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ?? throw new ArgumentNullException(nameof(gameScreenManager));

            EditSaveCommand = new AsyncRelayCommand(HandleEditSaveAsync, CanExecuteCommands);
            CancelCommand = new RelayCommand(CancelEditing, CanExecuteCommands);
            DeleteAccountCommand = new AsyncRelayCommand(DeleteAccountAsync, CanExecuteCommands);
            BackCommand = new RelayCommand(NavigateBack, CanExecuteCommands);
            ChangeAvatarCommand = new RelayCommand(ChangeAvatar, CanExecuteCommands);
            OpenPasswordModalCommand = new RelayCommand(() => IsPasswordModalVisible = true, CanExecuteCommands);
            ClosePasswordModalCommand = new RelayCommand(() => IsPasswordModalVisible = false, CanExecuteCommands);
        }

        public string DisplayName { get => displayName; set => SetProperty(ref displayName, value); }
        public string Email { get => email; set => SetProperty(ref email, value); }
        public string CurrentPassword { get => currentPassword; set => SetProperty(ref currentPassword, value); }
        public string NewPassword { get => newPassword; set => SetProperty(ref newPassword, value); }
        public bool IsPasswordModalVisible { get => isPasswordModalVisible; set => SetProperty(ref isPasswordModalVisible, value); }
        public string AvatarId
        {
            get => avatarId;
            set { if (SetProperty(ref avatarId, value)) OnPropertyChanged(nameof(AvatarImageSource)); }
        }

        public string AvatarImageSource => $"/GuessWhoClient;component/Presentation/Resources/Avatars/{AvatarId}.png";
        public string EditButtonText => IsEditing ? localizationService.Get("Save") : localizationService.Get("Edit");

        public bool IsEditing
        {
            get => isEditing;
            set { if (SetProperty(ref isEditing, value)) OnPropertyChanged(nameof(EditButtonText)); }
        }

        public AsyncRelayCommand EditSaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public AsyncRelayCommand DeleteAccountCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand ChangeAvatarCommand { get; }
        public RelayCommand OpenPasswordModalCommand { get; }
        public RelayCommand ClosePasswordModalCommand { get; }

        public async Task LoadProfileAsync()
        {
            IsBusy = true;
            try
            {
                var result = await profileAppService.GetProfileAsync(new GetProfileRequest { UserId = sessionContext.UserId });
                if (result != null && result.IsSuccess && result.Value != null)
                {
                    DisplayName = result.Value.Username;
                    Email = result.Value.Email;
                    AvatarId = string.IsNullOrEmpty(result.Value.AvatarId) ? "Avatar01" : result.Value.AvatarId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally { IsBusy = false; }
        }

        private async Task HandleEditSaveAsync()
        {
            if (!IsEditing)
            {
                IsEditing = true;
                return;
            }
            await UpdateProfileAsync();
        }

        private async Task UpdateProfileAsync()
        {
            var passwordDraft = new PasswordChangeDraft(CurrentPassword, NewPassword);
            var profileDraft = new ProfileUpdateDraft(DisplayName, AvatarId, passwordDraft);

            var validationErrors = UserRules.Validate(profileDraft);
            if (validationErrors.Count > 0)
            {
                alertService.Error(localizationService.Get(validationErrors[0].Key), localizationService.Get(KEY_UI_ERROR_TITLE));
                return;
            }

            IsBusy = true;
            try
            {
                var request = new UpdateProfileRequest
                {
                    UserId = sessionContext.UserId,
                    NewDisplayName = DisplayName,
                    NewAvatarId = AvatarId,
                    CurrentPasswordPlain = CurrentPassword,
                    NewPasswordPlain = NewPassword
                };

                var result = await profileAppService.UpdateProfileAsync(request);
                if (result != null && result.IsSuccess)
                {
                    alertService.Info(localizationService.Get("ProfileUpdateSuccess"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                    sessionContext.UpdateDisplayName(DisplayName);
                    IsEditing = false;
                    ClearPasswords();
                }
                else ShowProfileError(result?.FaultCode);
            }
            catch (Exception ex) { Logger.Error(ex); ShowUiError(KEY_GENERIC_ERROR); }
            finally { IsBusy = false; }
        }

        private async Task DeleteAccountAsync()
        {
            IsBusy = true;
            try
            {
                var result = await profileAppService.DeleteProfileAsync(new DeleteProfileRequest { UserId = sessionContext.UserId });
                if (result != null && result.IsSuccess)
                {
                    alertService.Info(localizationService.Get("AccountDeletedSuccess"), localizationService.Get(KEY_UI_SUCCESS_TITLE));
                    gameScreenManager.ShowScreen(GameScreenType.Login);
                }
                else ShowProfileError(result?.FaultCode);
            }
            catch (Exception ex) { Logger.Error(ex); ShowUiError(KEY_GENERIC_ERROR); }
            finally { IsBusy = false; }
        }

        private void ChangeAvatar()
        {
            if (string.IsNullOrEmpty(AvatarId)) AvatarId = "Avatar01";
            int current = int.Parse(AvatarId.Replace("Avatar", ""));
            int next = (current % 3) + 1;
            AvatarId = $"Avatar{next:D2}";
        }

        private void CancelEditing()
        {
            IsEditing = false;
            IsPasswordModalVisible = false;
            ClearPasswords();
            _ = LoadProfileAsync();
        }

        private void ClearPasswords()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
        }

        private void NavigateBack() => gameScreenManager.ShowScreen(GameScreenType.MainMenu);

        private void ShowProfileError(string faultCode)
        {
            var mapping = profileFaultMapper.Map(faultCode);
            alertService.Error(localizationService.Get(mapping.IsMapped ? mapping.UiKey : KEY_GENERIC_ERROR), localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private void ShowUiError(string messageKey) => alertService.Error(localizationService.Get(messageKey), localizationService.Get(KEY_UI_ERROR_TITLE));

        private bool CanExecuteCommands() => !IsBusy;

        protected override void OnIsBusyChanged(string propertyName)
        {
            EditSaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            DeleteAccountCommand.RaiseCanExecuteChanged();
            BackCommand.RaiseCanExecuteChanged();
            ChangeAvatarCommand.RaiseCanExecuteChanged();
        }
    }
}