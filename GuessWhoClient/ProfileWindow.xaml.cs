using GuessWhoClient.Alerts;
using GuessWhoClient.Assets;
using GuessWhoClient.Dtos;
using GuessWhoClient.Globalization;
using GuessWhoClient.InputValidation;
using GuessWhoClient.Session;
using GuessWhoClient.UpdateServiceRef;
using GuessWhoClient.Windows;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class ProfileWindow : UserControl, INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProfileWindow));

        private const string EDIT_BUTTON_TEXT_SAVE = "Save";
        private const string EDIT_BUTTON_TEXT_EDIT = "Edit";

        private const string PASSWORD_CHANGED_SUCCESSFULLY_MESSAGE = "Password changed successfully.";
        private const string PASSWORD_CHANGED_ERROR_MESSAGE = "Password could not be changed. Verify current password.";

        private const string AVATAR_UPDATED_SUCCESSFULLY_MESSAGE = "Avatar updated successfully.";
        private const string AVATAR_UPDATED_ERROR_MESSAGE = "Avatar could not be updated.";
        private const string AVATAR_ID_EMPTY_ERROR_MESSAGE = "AvatarId cannot be empty.";

        private const string ACCOUNT_DELETED_ERROR_MESSAGE = "Account could not be deleted.";
        private const string ACCOUNT_DELETED_SUCCESS_MESSAGE = "Account deleted successfully.";

        private const string UNEXPECTED_LOAD_PROFILE_ERROR_MESSAGE = "An unexpected error occurred while loading the profile.";
        private const string UNEXPECTED_UPDATE_PROFILE_ERROR_MESSAGE = "An unexpected error occurred while updating the profile.";
        private const string UNEXPECTED_CHANGE_PASSWORD_ERROR_MESSAGE = "An unexpected error occurred while changing the password.";
        private const string UNEXPECTED_CHANGE_AVATAR_ERROR_MESSAGE = "An unexpected error occurred while updating the avatar.";
        private const string UNEXPECTED_DELETE_ACCOUNT_ERROR_MESSAGE = "An unexpected error occurred while deleting the account.";

        private const string TIMEOUT_GENERIC_MESSAGE = "The operation timed out while communicating with the server.";
        private const string COMMUNICATION_GENERIC_ERROR_MESSAGE = "A communication error occurred while contacting the server.";

        private const string FAULT_UNEXPECTED_KEY = "FaultUnexpected";

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        private static readonly long CURRENT_USER_ID = SessionContext.Current.UserId;

        private bool isEditing;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private string avatarId;

        public string AvatarId
        {
            get => avatarId;
            set
            {
                if (avatarId == value)
                {
                    return;
                }

                avatarId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvatarImageSource));
            }
        }

        public string AvatarImageSource => AvatarAssets.GetAvatarPathById(AvatarId);

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEditing
        {
            get => isEditing;
            set
            {
                if (isEditing == value)
                {
                    return;
                }

                isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditButtonText));
            }
        }

        public string EditButtonText => IsEditing ? EDIT_BUTTON_TEXT_SAVE : EDIT_BUTTON_TEXT_EDIT;

        public string DisplayName
        {
            get => displayName;
            set
            {
                if (displayName == value)
                {
                    return;
                }

                displayName = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (email == value)
                {
                    return;
                }

                email = value;
                OnPropertyChanged();
            }
        }

        public ProfileWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += (_, __) => LoadCurrentUserProfile();

            btnEditSave.Click += OnEditSaveButtonClick;
            btnCancel.Click += OnCancelEditButtonClick;
            btnChangePassword.Click += OnChangePasswordButtonClick;
            btnBack.Click += OnBackButtonClick;
            btnChangeAvatar.Click += OnChangeAvatarButtonClick;
            btnDeleteAccount.Click += OnDeleteAccountButtonClick;
        }

        private void LoadCurrentUserProfile()
        {
            try
            {
                using (var updateProfileClient = new UpdateProfileServiceClient())
                {
                    var getProfileRequest = new GetProfileRequest
                    {
                        UserId = CURRENT_USER_ID
                    };

                    var getProfileResponse = updateProfileClient.GetProfile(getProfileRequest);

                    DisplayName = getProfileResponse.Username ?? string.Empty;
                    Email = getProfileResponse.Email ?? string.Empty;
                    AvatarId = getProfileResponse.AvatarId;

                    IsEditing = false;
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while loading user profile.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while loading user profile.", ex);
                alertService.Error(TIMEOUT_GENERIC_MESSAGE);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while loading user profile.", ex);
                alertService.Error(COMMUNICATION_GENERIC_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in LoadCurrentUserProfile.", ex);
                alertService.Error(UNEXPECTED_LOAD_PROFILE_ERROR_MESSAGE);
            }
        }

        private void OnEditSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                IsEditing = true;
                return;
            }

            TrySaveProfileChanges();
        }

        private void OnCancelEditButtonClick(object sender, RoutedEventArgs e)
        {
            LoadCurrentUserProfile();
        }

        private void TrySaveProfileChanges()
        {
            try
            {
                UpdateProfileRequest updateProfileRequest = BuildUpdateProfileRequestFromForm();

                using (var updateProfileClient = new UpdateProfileServiceClient())
                {
                    UpdateProfileResponse updateProfileResponse = updateProfileClient.UpdateUserProfile(updateProfileRequest);

                    if (updateProfileResponse.Updated)
                    {
                        DisplayName = updateProfileResponse.Username ?? DisplayName;
                        Email = updateProfileResponse.Email ?? Email;
                        IsEditing = false;
                    }
                    else
                    {
                        Logger.Warn("Profile update response returned Updated = false.");
                        alertService.Error(UNEXPECTED_UPDATE_PROFILE_ERROR_MESSAGE);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn("Validation error while updating profile.", ex);
                alertService.Warn(ex.Message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while updating profile.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while updating profile.", ex);
                alertService.Error(TIMEOUT_GENERIC_MESSAGE);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while updating profile.", ex);
                alertService.Error(COMMUNICATION_GENERIC_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in TrySaveProfileChanges.", ex);
                alertService.Error(UNEXPECTED_UPDATE_PROFILE_ERROR_MESSAGE);
            }
        }

        private UpdateProfileRequest BuildUpdateProfileRequestFromForm()
        {
            var profileInput = new AccountProfileInput(
                Email,
                DisplayName,
                password: string.Empty,
                confirmPassword: string.Empty);

            List<string> validationErrors = AccountValidator.ValidateProfileWithoutPassword(profileInput);

            if (validationErrors.Count > 0)
            {
                string errorMessage = string.Join(Environment.NewLine, validationErrors);
                throw new InvalidOperationException(errorMessage);
            }

            return new UpdateProfileRequest
            {
                UserId = CURRENT_USER_ID,
                NewDisplayName = profileInput.DisplayName,
                CurrentPasswordPlain = null,
                NewPasswordPlain = null
            };
        }

        private void OnChangePasswordButtonClick(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                return;
            }

            var changePasswordDialog = new ChangePasswordDialog();

            EventHandler<bool> dialogClosedHandler = null;
            dialogClosedHandler = (s, accepted) =>
            {
                changePasswordDialog.PasswordChangeDialogClosed -= dialogClosedHandler;
                HandlePasswordDialogClosed(gameWindow, changePasswordDialog, accepted);
            };

            changePasswordDialog.PasswordChangeDialogClosed += dialogClosedHandler;
            gameWindow.ShowScreen(changePasswordDialog);
        }

        private void HandlePasswordDialogClosed(GameWindow gameWindow, ChangePasswordDialog changePasswordDialog, bool accepted)
        {
            try
            {
                if (!accepted || changePasswordDialog.PasswordChangeRequest == null)
                {
                    return;
                }

                PasswordChangeRequest passwordChangeRequest = changePasswordDialog.PasswordChangeRequest;
                TryChangePassword(passwordChangeRequest);
            }
            finally
            {
                gameWindow.ShowScreen(this);
            }
        }

        private void TryChangePassword(PasswordChangeRequest passwordChangeRequest)
        {
            try
            {
                UpdateProfileRequest updatePasswordRequest = BuildPasswordChangeUpdateRequest(passwordChangeRequest);

                using (var updateProfileClient = new UpdateProfileServiceClient())
                {
                    var updateProfileResponse = updateProfileClient.UpdateUserProfile(updatePasswordRequest);

                    if (updateProfileResponse.Updated)
                    {
                        alertService.Info(PASSWORD_CHANGED_SUCCESSFULLY_MESSAGE);
                    }
                    else
                    {
                        alertService.Error(PASSWORD_CHANGED_ERROR_MESSAGE);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while changing password.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while changing password.", ex);
                alertService.Error(TIMEOUT_GENERIC_MESSAGE);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while changing password.", ex);
                alertService.Error(COMMUNICATION_GENERIC_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error while changing password.", ex);
                alertService.Error(UNEXPECTED_CHANGE_PASSWORD_ERROR_MESSAGE);
            }
        }

        private UpdateProfileRequest BuildPasswordChangeUpdateRequest(PasswordChangeRequest passwordChangeRequest)
        {
            if (passwordChangeRequest == null)
            {
                throw new ArgumentNullException(nameof(passwordChangeRequest));
            }

            return new UpdateProfileRequest
            {
                UserId = CURRENT_USER_ID,
                NewDisplayName = null,
                CurrentPasswordPlain = passwordChangeRequest.CurrentPassword,
                NewPasswordPlain = passwordChangeRequest.NewPassword
            };
        }

        private void OnChangeAvatarButtonClick(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                return;
            }

            var chooseAvatarWindow = new ChooseAvatarWindow();

            EventHandler<bool> avatarSelectedHandler = null;
            avatarSelectedHandler = (s, accepted) =>
            {
                chooseAvatarWindow.AvatarChangeClose -= avatarSelectedHandler;
                HandleAvatarDialogClosed(gameWindow, chooseAvatarWindow, accepted);
            };

            chooseAvatarWindow.AvatarChangeClose += avatarSelectedHandler;
            gameWindow.ShowScreen(chooseAvatarWindow);
        }

        private void HandleAvatarDialogClosed(GameWindow gameWindow, ChooseAvatarWindow chooseAvatarWindow, bool accepted)
        {
            try
            {
                if (!accepted || chooseAvatarWindow.AvatarChangeRequest == null)
                {
                    return;
                }

                AvatarChangeRequest avatarChangeRequest = chooseAvatarWindow.AvatarChangeRequest;
                TryChangeAvatar(avatarChangeRequest);
            }
            finally
            {
                gameWindow.ShowScreen(this);
            }
        }

        private void TryChangeAvatar(AvatarChangeRequest avatarChangeRequest)
        {
            if (avatarChangeRequest == null)
            {
                throw new ArgumentNullException(nameof(avatarChangeRequest));
            }

            try
            {
                UpdateProfileRequest updateAvatarRequest = BuildAvatarUpdateRequest(avatarChangeRequest);

                using (var updateProfileClient = new UpdateProfileServiceClient())
                {
                    var updateProfileResponse = updateProfileClient.UpdateUserProfile(updateAvatarRequest);

                    if (!updateProfileResponse.Updated)
                    {
                        Logger.Error("Avatar could not be updated.");
                        alertService.Error(AVATAR_UPDATED_ERROR_MESSAGE);
                    }
                    else
                    {
                        AvatarId = avatarChangeRequest.AvatarId;
                        alertService.Info(AVATAR_UPDATED_SUCCESSFULLY_MESSAGE);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while updating avatar.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while updating avatar.", ex);
                alertService.Error(TIMEOUT_GENERIC_MESSAGE);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while updating avatar.", ex);
                alertService.Error(COMMUNICATION_GENERIC_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error while updating avatar.", ex);
                alertService.Error(UNEXPECTED_CHANGE_AVATAR_ERROR_MESSAGE);
            }
        }

        private UpdateProfileRequest BuildAvatarUpdateRequest(AvatarChangeRequest avatarChangeRequest)
        {
            if (avatarChangeRequest == null)
            {
                throw new ArgumentNullException(nameof(avatarChangeRequest));
            }

            if (string.IsNullOrWhiteSpace(avatarChangeRequest.AvatarId))
            {
                throw new InvalidOperationException(AVATAR_ID_EMPTY_ERROR_MESSAGE);
            }

            return new UpdateProfileRequest
            {
                UserId = CURRENT_USER_ID,
                NewDisplayName = null,
                CurrentPasswordPlain = null,
                NewPasswordPlain = null,
                NewAvatarId = avatarChangeRequest.AvatarId
            };
        }

        private void OnDeleteAccountButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var updateProfileClient = new UpdateProfileServiceClient())
                {
                    var deleteProfileRequest = new DeleteProfileRequest
                    {
                        UserId = CURRENT_USER_ID
                    };

                    var deleteProfileResponse = updateProfileClient.DeleteUserProfile(deleteProfileRequest);

                    if (deleteProfileResponse.Success)
                    {
                        Logger.Info("Account deleted successfully.");
                        alertService.Info(ACCOUNT_DELETED_SUCCESS_MESSAGE);

                        SessionContext.Current.SignOut();
                        var gameWindow = Window.GetWindow(this) as GameWindow;
                        gameWindow?.LoadMainMenu();
                    }
                    else
                    {
                        Logger.Error("Account could not be deleted.");
                        alertService.Error(ACCOUNT_DELETED_ERROR_MESSAGE);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while deleting account.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    FAULT_UNEXPECTED_KEY);

                alertService.Error(localizedText);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("Timeout while deleting account.", ex);
                alertService.Error(TIMEOUT_GENERIC_MESSAGE);
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Communication error while deleting account.", ex);
                alertService.Error(COMMUNICATION_GENERIC_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in OnDeleteAccountButtonClick.", ex);
                alertService.Error(UNEXPECTED_DELETE_ACCOUNT_ERROR_MESSAGE);
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;
            gameWindow?.LoadMainMenu();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
