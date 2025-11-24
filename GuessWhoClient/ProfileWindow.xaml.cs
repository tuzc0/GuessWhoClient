using GuessWhoClient.Alerts;
using GuessWhoClient.Assets;
using GuessWhoClient.Globalization;
using GuessWhoClient.Session;
using GuessWhoClient.UpdateServiceRef;
using GuessWhoClient.Windows;
using log4net;
using System;
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

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        private static readonly long userId = SessionContext.Current.UserId;

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

        public string EditButtonText => IsEditing ? "Save" : "Edit";

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
            btnDeleteAccount.Click += OnDeleteAccountButtonClick;
        }

        private void LoadCurrentUserProfile()
        {
            try
            {
                using (var client = new UpdateProfileServiceClient())
                {
                    var request = new GetProfileRequest
                    {
                        UserId = userId
                    };

                    var response = client.GetProfile(request);

                    DisplayName = response.Username ?? string.Empty;
                    Email = response.Email ?? string.Empty;

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
                    "FaultUnexpected");

                alertService.Error(localizedText);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in LoadCurrentUserProfile.", ex);
                alertService.Error("An unexpected error occurred while loading the profile.");
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
                using (var client = new UpdateProfileServiceClient())
                {
                    UpdateProfileRequest updateRequest = BuildUpdateProfileRequestFromForm(userId);
                    UpdateProfileResponse updateResponse = client.UpdateUserProfile(updateRequest);

                    if (updateResponse.Updated)
                    {
                        DisplayName = updateResponse.Username ?? DisplayName;
                        Email = updateResponse.Email ?? Email;
                        IsEditing = false;
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while updating profile.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    "FaultUnexpected");

                alertService.Error(localizedText);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in TrySaveProfileChanges.", ex);
            }
        }

        private void OnChangePasswordButtonClick(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;

            if (ownerWindow == null)
            {
                return;
            }

            var changePasswordDialog = new ChangePasswordDialog();

            EventHandler<bool> dialogClosedHandler = null;
            dialogClosedHandler = (s, accepted) =>
            {
                try
                {
                    changePasswordDialog.PasswordChangeDialogClosed -= dialogClosedHandler;

                    if (!accepted)
                    {
                        ownerWindow.ShowScreen(this);
                        return;
                    }

                    if (changePasswordDialog.NewPassword != changePasswordDialog.ConfirmNewPassword)
                    {
                        MessageBox.Show(
                            "Passwords do not match.",
                            "Validation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        ownerWindow.ShowScreen(this);
                        return;
                    }

                    using (var client = new UpdateProfileServiceClient())
                    {
                        var request = new UpdateProfileRequest
                        {
                            UserId = userId,
                            NewDisplayName = null,
                            CurrentPasswordPlain = changePasswordDialog.CurrentPassword,
                            NewPasswordPlain = changePasswordDialog.NewPassword
                        };

                        var response = client.UpdateUserProfile(request);

                        if (response.Updated)
                        {
                            MessageBox.Show(
                                "Password changed successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Password could not be changed. Verify current password.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                finally
                {
                    ownerWindow.ShowScreen(this);
                }
            };

            changePasswordDialog.PasswordChangeDialogClosed += dialogClosedHandler;
            ownerWindow.ShowScreen(changePasswordDialog);
        }

        private void OnDeleteAccountButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new UpdateProfileServiceClient())
                {
                    var request = new DeleteProfileRequest
                    {
                        UserId = userId
                    };

                    var response = client.DeleteUserProfile(request);

                    if (response.Success)
                    {
                        SessionContext.Current.SignOut();
                        var ownerWindow = Window.GetWindow(this) as GameWindow;
                        ownerWindow?.LoadMainMenu();
                    }
                    else
                    {
                        Logger.Error("Account could not be deleted.");
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn("Service fault while delete account.", ex);

                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    "FaultUnexpected");

                alertService.Error(localizedText);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in OnDeleteAccountButtonClick.", ex);
            }
        }

        private UpdateProfileRequest BuildUpdateProfileRequestFromForm(long userId)
        {
            string trimmedDisplayName = DisplayName == null
                ? string.Empty
                : DisplayName.Trim();

            if (string.IsNullOrWhiteSpace(trimmedDisplayName))
            {
                throw new InvalidOperationException("Display name cannot be empty.");
            }

            return new UpdateProfileRequest
            {
                UserId = userId,
                NewDisplayName = trimmedDisplayName,
                CurrentPasswordPlain = null,
                NewPasswordPlain = null
            };
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            var ownerWindow = Window.GetWindow(this) as GameWindow;
            ownerWindow?.LoadMainMenu();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
