using GuessWhoClient.Assets;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public sealed class UpdateProfileViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateProfileViewModel));

        private const string LOG_CTX_LOAD = "UpdateProfileViewModel.LoadProfile";
        private const string LOG_CTX_UPDATE = "UpdateProfileViewModel.UpdateProfile";
        private const string LOG_CTX_DELETE = "UpdateProfileViewModel.DeleteProfile";
        private const string LOG_CTX_VERIFY_BTN = "UpdateProfileViewModel.VerifyEmail";
        private const string KEY_UI_DELETE_ACCOUNT_CONFIRM = "UiDeleteAccountConfirm";

        private const string KEY_UI_TITLE_ERROR = "UiTitleError";
        private const string KEY_UI_TITLE_INFO = "UiTitleInfo";
        private const string KEY_UI_TITLE_WARNING = "UiTitleWarning";
        private const string KEY_UI_GENERIC_ERROR = "UiGenericError";

        private const string KEY_BTN_EDIT = "BtnEdit";
        private const string KEY_BTN_SAVE = "BtnSave";
        private const string KEY_BTN_YES = "BtnYes";
        private const string KEY_BTN_NO = "BtnNo";

        private const string KEY_UI_PROFILE_UPDATE_SUCCESS = "UiProfileUpdateSuccess";
        private const string KEY_UI_PROFILE_DELETE_SUCCESS = "UiProfileDeleteSuccess";

        private const string KEY_UI_EMAIL_NOT_VERIFIED = "UiEmailNotVerified";

        private const string PACK_URI_PREFIX = "pack://application:,,,";
        private const string EMPTY = "";

        private const string DEFAULT_AVATAR_ID = "A0001";

        private readonly IUpdateProfileAppService profileAppService;
        private readonly IAlertService alertService;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;
        private readonly IGameScreenManager gameScreenManager;
        private readonly IAvatarPathResolver avatarPathResolver;
        private readonly IOverlayDialogService overlayDialogService;
        private readonly AccountFlowContext accountFlowContext;
        private readonly IValidationIssueMapper validationIssueMapper;
        private readonly IGameConfirmDialogService confirmDialogService;

        private long accountId;
        private bool isEmailVerified;

        private string displayName;
        private string email;
        private string avatarId;
        private bool isEditing;

        private string currentPassword;
        private string newPassword;

        private ImageSource avatarImageSource;

        public UpdateProfileViewModel(
            IUpdateProfileAppService profileAppService,
            IAlertService alertService,
            IUiFaultMapper uiFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext,
            IGameScreenManager gameScreenManager,
            IAvatarPathResolver avatarPathResolver,
            IOverlayDialogService overlayDialogService,
            AccountFlowContext accountFlowContext,
            IValidationIssueMapper validationIssueMapper,
            IGameConfirmDialogService confirmDialogService)
        {
            this.profileAppService = profileAppService ??
                throw new ArgumentNullException(nameof(profileAppService));
            this.alertService = alertService ??
                throw new ArgumentNullException(nameof(alertService));
            this.uiFaultMapper = uiFaultMapper ??
                throw new ArgumentNullException(nameof(uiFaultMapper));
            this.localizationService = localizationService ??
                throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
            this.gameScreenManager = gameScreenManager ??
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.avatarPathResolver = avatarPathResolver ??
                throw new ArgumentNullException(nameof(avatarPathResolver));
            this.overlayDialogService = overlayDialogService ??
                throw new ArgumentNullException(nameof(overlayDialogService));
            this.accountFlowContext = accountFlowContext ??
                throw new ArgumentNullException(nameof(accountFlowContext));
            this.validationIssueMapper = validationIssueMapper ??
                throw new ArgumentNullException(nameof(validationIssueMapper));
            this.confirmDialogService = confirmDialogService ??
                throw new ArgumentNullException(nameof(confirmDialogService));


            currentPassword = EMPTY;
            newPassword = EMPTY;

            EditSaveCommand = new AsyncRelayCommand(HandleEditSaveAsync, CanExecuteEditCommands);
            CancelCommand = new RelayCommand(CancelEditing, CanExecuteAlways);
            DeleteAccountCommand = new AsyncRelayCommand(DeleteAccountAsync, CanExecuteAlways);
            BackCommand = new RelayCommand(Back, CanExecuteAlways);

            ChangeAvatarCommand = new RelayCommand(ChangeAvatar, CanExecuteEditCommands);
            ChangePasswordCommand = new RelayCommand(ChangePassword, CanExecuteEditCommands);

            VerifyEmailCommand = new RelayCommand(OpenVerifyEmailOverlay, CanExecuteVerifyEmail);
            this.confirmDialogService = confirmDialogService;
        }

        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string AvatarId
        {
            get => avatarId;
            set
            {
                if (SetProperty(ref avatarId, value))
                {
                    UpdateAvatarImage();
                }
            }
        }

        public ImageSource AvatarImageSource
        {
            get => avatarImageSource;
            private set => SetProperty(ref avatarImageSource, value);
        }

        public bool IsEditing
        {
            get => isEditing;
            set
            {
                if (SetProperty(ref isEditing, value))
                {
                    OnPropertyChanged(nameof(EditButtonText));
                }
            }
        }

        public bool IsEmailVerified
        {
            get => isEmailVerified;
            private set
            {
                if (SetProperty(ref isEmailVerified, value))
                {
                    OnPropertyChanged(nameof(IsEmailNotVerified));

                    if (!isEmailVerified && IsEditing)
                    {
                        IsEditing = false;
                    }

                    RaiseAllCanExecuteChanged();
                }
            }
        }

        public bool IsEmailNotVerified => !IsEmailVerified;

        public string EditButtonText =>
            IsEditing ? localizationService.Get(KEY_BTN_SAVE) : localizationService.Get(KEY_BTN_EDIT);

        public AsyncRelayCommand EditSaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public AsyncRelayCommand DeleteAccountCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand ChangeAvatarCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand VerifyEmailCommand { get; }

        public async Task LoadProfileAsync()
        {
            IsBusy = true;

            try
            {
                var request = new GetProfileRequest
                {
                    UserId = sessionContext.UserId
                };

                var result = await profileAppService.GetProfileAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue || result.Value == null)
                {
                    ShowProfileError(result?.FaultCode);
                    return;
                }

                accountId = result.Value.AccountId;
                IsEmailVerified = result.Value.IsEmailVerified;

                DisplayName = (result.Value.Username ?? EMPTY).Trim();
                Email = result.Value.Email ?? EMPTY;

                string resolvedAvatarId = string.IsNullOrWhiteSpace(result.Value.AvatarId)
                    ? DEFAULT_AVATAR_ID
                    : result.Value.AvatarId.Trim();

                AvatarId = resolvedAvatarId;

                sessionContext.UpdateDisplayName(DisplayName);
                sessionContext.UpdateAvatarId(AvatarId);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_LOAD, ex);
                ShowUiError(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task HandleEditSaveAsync()
        {
            if (!IsEmailVerified)
            {
                WarnEmailNotVerified();
                return;
            }

            if (!IsEditing)
            {
                IsEditing = true;
                return;
            }

            await UpdateProfileAsync();
        }

        private async Task UpdateProfileAsync()
        {
            if (!IsEmailVerified)
            {
                WarnEmailNotVerified();
                return;
            }

            ProfileUpdateDraft draft = BuildProfileUpdateDraftFromState();

            if (!ValidateDisplayNameRequiredOrWarn(draft))
            {
                return;
            }

            if (!ValidateDraftOrWarn(draft))
            {
                return;
            }

            await ExecuteUpdateProfileAsync(draft);
        }

        private ProfileUpdateDraft BuildProfileUpdateDraftFromState()
        {
            string safeDisplayName = (DisplayName ?? EMPTY).Trim();
            string safeAvatarId = (AvatarId ?? EMPTY).Trim();

            string safeCurrentPassword = currentPassword ?? EMPTY;
            string safeNewPassword = newPassword ?? EMPTY;

            var passwordChange = new PasswordChangeDraft(safeCurrentPassword, safeNewPassword);

            return new ProfileUpdateDraft(
                displayName: safeDisplayName,
                avatarId: safeAvatarId,
                passwordChange: passwordChange);
        }

        private bool ValidateDisplayNameRequiredOrWarn(ProfileUpdateDraft draft)
        {
            if (!string.IsNullOrWhiteSpace(draft.DisplayName))
            {
                return true;
            }

            WarnValidationCode(UserValidationCodes.DISPLAY_NAME_REQUIRED);
            return false;
        }

        private bool ValidateDraftOrWarn(ProfileUpdateDraft draft)
        {
            var validationErrors = UserRules.Validate(draft);

            if (validationErrors == null || validationErrors.Count == 0)
            {
                return true;
            }

            string code = (validationErrors[0].Key ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                ShowUiError(KEY_UI_GENERIC_ERROR);
                return false;
            }

            WarnValidationCode(code);
            return false;
        }

        private async Task ExecuteUpdateProfileAsync(ProfileUpdateDraft draft)
        {
            IsBusy = true;

            try
            {
                UpdateProfileRequest request = BuildUpdateProfileRequest(draft);

                var result = await profileAppService.UpdateProfileAsync(request);

                if (result != null && result.IsSuccess)
                {
                    HandleUpdateProfileSuccess(draft);
                    return;
                }

                ShowProfileError(result?.FaultCode);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_UPDATE, ex);
                ShowUiError(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private UpdateProfileRequest BuildUpdateProfileRequest(ProfileUpdateDraft draft)
        {
            PasswordChangeDraft passwordChange = draft.PasswordChange ?? new PasswordChangeDraft(EMPTY, EMPTY);

            return new UpdateProfileRequest
            {
                UserId = sessionContext.UserId,
                NewDisplayName = draft.DisplayName ?? EMPTY,
                NewAvatarId = draft.AvatarId ?? EMPTY,
                CurrentPasswordPlain = passwordChange.CurrentPassword ?? EMPTY,
                NewPasswordPlain = passwordChange.NewPassword ?? EMPTY
            };
        }

        private void HandleUpdateProfileSuccess(ProfileUpdateDraft draft)
        {
            alertService.Info(
                localizationService.Get(KEY_UI_PROFILE_UPDATE_SUCCESS),
                localizationService.Get(KEY_UI_TITLE_INFO));

            sessionContext.UpdateDisplayName(draft.DisplayName);
            sessionContext.UpdateAvatarId(draft.AvatarId);

            IsEditing = false;
            ClearPasswordDraft();
        }

        private void CancelEditing()
        {
            IsEditing = false;

            DisplayName = (sessionContext.DisplayName ?? EMPTY).Trim();
            AvatarId = string.IsNullOrWhiteSpace(sessionContext.AvatarId)
                ? DEFAULT_AVATAR_ID
                : sessionContext.AvatarId.Trim();

            ClearPasswordDraft();
        }

        private async Task DeleteAccountAsync()
        {
            if (!ConfirmDeleteAccount())
            {
                return;
            }

            IsBusy = true;

            try
            {
                var request = new DeleteProfileRequest { UserId = sessionContext.UserId };
                var result = await profileAppService.DeleteProfileAsync(request);

                if (result != null && result.IsSuccess)
                {
                    alertService.Info(
                        localizationService.Get(KEY_UI_PROFILE_DELETE_SUCCESS),
                        localizationService.Get(KEY_UI_TITLE_INFO));

                    gameScreenManager.ShowScreen(GameScreenType.Login);
                    return;
                }

                ShowProfileError(result?.FaultCode);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_DELETE, ex);
                ShowUiError(KEY_UI_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ConfirmDeleteAccount()
        {
            string title = localizationService.Get(KEY_UI_TITLE_WARNING) ?? EMPTY;
            string message = localizationService.Get(KEY_UI_DELETE_ACCOUNT_CONFIRM) ?? EMPTY;

            string yes = localizationService.Get(KEY_BTN_YES) ?? EMPTY;
            string no = localizationService.Get(KEY_BTN_NO) ?? EMPTY;

            return confirmDialogService.Confirm(title, message, yes, no);
        }


        private void ChangeAvatar()
        {
            if (!IsEmailVerified)
            {
                WarnEmailNotVerified();
                return;
            }

            overlayDialogService.ShowChooseAvatar(
                currentAvatarId: AvatarId,
                onSelected: selectedAvatarId =>
                {
                    string safeSelected = (selectedAvatarId ?? EMPTY).Trim();

                    if (!string.IsNullOrWhiteSpace(safeSelected))
                    {
                        AvatarId = safeSelected;
                        IsEditing = true;
                    }
                });
        }

        private void ChangePassword()
        {
            if (!IsEmailVerified)
            {
                WarnEmailNotVerified();
                return;
            }

            overlayDialogService.ShowChangePassword(draft =>
            {
                if (draft == null)
                {
                    return;
                }

                currentPassword = draft.CurrentPassword ?? EMPTY;
                newPassword = draft.NewPassword ?? EMPTY;

                IsEditing = true;
            });
        }

        private void OpenVerifyEmailOverlay()
        {
            try
            {
                if (accountId <= 0)
                {
                    ShowUiError(KEY_UI_GENERIC_ERROR);
                    return;
                }

                string safeEmail = (Email ?? EMPTY).Trim();

                if (string.IsNullOrWhiteSpace(safeEmail))
                {
                    ShowUiError(KEY_UI_GENERIC_ERROR);
                    return;
                }

                accountFlowContext.SetPendingEmailVerification(accountId, safeEmail, GameScreenType.UpdateProfile);
                gameScreenManager.ShowOverlay(GameScreenType.VerifyEmail);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_VERIFY_BTN, ex);
                ShowUiError(KEY_UI_GENERIC_ERROR);
            }
        }

        private void Back()
        {
            gameScreenManager.ShowScreen(GameScreenType.MainMenu);
        }

        private void UpdateAvatarImage()
        {
            string safeAvatarId = string.IsNullOrWhiteSpace(AvatarId) ? DEFAULT_AVATAR_ID : AvatarId.Trim();

            try
            {
                if (TrySetAvatarImage(safeAvatarId))
                {
                    return;
                }

                TrySetAvatarImage(DEFAULT_AVATAR_ID);
            }
            catch (UriFormatException)
            {
                TrySetAvatarImage(DEFAULT_AVATAR_ID);
            }
            catch (NotSupportedException)
            {
                TrySetAvatarImage(DEFAULT_AVATAR_ID);
            }
            catch (IOException)
            {
                TrySetAvatarImage(DEFAULT_AVATAR_ID);
            }
        }

        private bool TrySetAvatarImage(string avatarIdValue)
        {
            Uri uri = TryResolveAvatarUri(avatarIdValue);

            if (uri == null)
            {
                return false;
            }

            AvatarImageSource = CreateBitmap(uri);
            return true;
        }

        private Uri TryResolveAvatarUri(string avatarIdValue)
        {
            string path = (avatarPathResolver.Resolve(avatarIdValue) ?? EMPTY).Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.StartsWith(PACK_URI_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(path, UriKind.Absolute);
            }

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                return new Uri(PACK_URI_PREFIX + path, UriKind.Absolute);
            }

            if (Path.IsPathRooted(path))
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return new Uri(path, UriKind.Absolute);
            }

            return new Uri(PACK_URI_PREFIX + "/" + path, UriKind.Absolute);
        }

        private static BitmapImage CreateBitmap(Uri uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void ClearPasswordDraft()
        {
            currentPassword = EMPTY;
            newPassword = EMPTY;
        }

        private void WarnValidationCode(string validationCode)
        {
            string uiKey = ResolveValidationUiKey(validationCode);

            if (string.IsNullOrWhiteSpace(uiKey))
            {
                uiKey = KEY_UI_GENERIC_ERROR;
            }

            string title = localizationService.Get(KEY_UI_TITLE_WARNING) ?? EMPTY;
            string message = localizationService.Get(uiKey) ?? EMPTY;

            if (string.IsNullOrWhiteSpace(message))
            {
                message = localizationService.Get(KEY_UI_GENERIC_ERROR) ?? EMPTY;
            }

            alertService.Warn(message, title);
        }

        private string ResolveValidationUiKey(string code)
        {
            string safeCode = (code ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeCode))
            {
                return EMPTY;
            }

            ValidationIssueMapping mapping = validationIssueMapper.Map(safeCode);

            if (mapping.IsMapped && !string.IsNullOrWhiteSpace(mapping.MessageKey))
            {
                return (mapping.MessageKey ?? EMPTY).Trim();
            }

            UiKeyMapping fallback = uiFaultMapper.Map(safeCode);

            return fallback.IsMapped
                ? (fallback.UiKey ?? EMPTY).Trim()
                : EMPTY;
        }

        private void WarnEmailNotVerified()
        {
            alertService.Warn(
                localizationService.Get(KEY_UI_EMAIL_NOT_VERIFIED),
                localizationService.Get(KEY_UI_TITLE_WARNING));
        }

        private void ShowProfileError(string faultCode)
        {
            UiKeyMapping mapping = uiFaultMapper.Map(faultCode);
            string key = mapping.IsMapped ? mapping.UiKey : KEY_UI_GENERIC_ERROR;

            alertService.Error(
                localizationService.Get(key),
                localizationService.Get(KEY_UI_TITLE_ERROR));
        }

        private void ShowUiError(string messageKey)
        {
            alertService.Error(
                localizationService.Get(messageKey),
                localizationService.Get(KEY_UI_TITLE_ERROR));
        }

        private bool CanExecuteAlways()
        {
            return !IsBusy;
        }

        private bool CanExecuteEditCommands()
        {
            return !IsBusy && IsEmailVerified;
        }

        private bool CanExecuteVerifyEmail()
        {
            return !IsBusy && IsEmailNotVerified;
        }

        private void RaiseAllCanExecuteChanged()
        {
            EditSaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            DeleteAccountCommand.RaiseCanExecuteChanged();
            BackCommand.RaiseCanExecuteChanged();
            ChangeAvatarCommand.RaiseCanExecuteChanged();
            ChangePasswordCommand.RaiseCanExecuteChanged();
            VerifyEmailCommand.RaiseCanExecuteChanged();
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            RaiseAllCanExecuteChanged();
        }
    }
}