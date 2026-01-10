using GuessWhoClient.Assets;
using GuessWhoClient.Globalization;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public sealed class ChooseAvatarViewModel : ViewModelBase
    {
        private const string EMPTY = "";

        private const string KEY_UI_TITLE_INFO = "UiTitleInfo";
        private const string KEY_UI_SELECT_REQUIRED = "UiChooseAvatarSelectRequired";

        private readonly IAlertService alertService;
        private readonly ILocalizationService localizationService;

        private AvatarCardViewModel selectedAvatar;

        public ChooseAvatarViewModel(IAlertService alertService, ILocalizationService localizationService)
        {
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            ConfirmCommand = new RelayCommand(Confirm, CanExecuteCommands);
            CancelCommand = new RelayCommand(Cancel, CanExecuteCommands);
        }

        public ObservableCollection<AvatarCardViewModel> AvatarCards { get; } =
            new ObservableCollection<AvatarCardViewModel>();

        public AvatarCardViewModel SelectedAvatar
        {
            get => selectedAvatar;
            set
            {
                if (SetProperty(ref selectedAvatar, value))
                {
                    ConfirmCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action<string> AvatarSelected { get; set; }
        public Action RequestClose { get; set; }

        public void Initialize(string currentAvatarId)
        {
            AvatarCards.Clear();

            foreach (var avatar in AvatarAssets.GetAllAvatars())
            {
                AvatarCards.Add(new AvatarCardViewModel(avatar.Key, avatar.Value));
            }

            SelectedAvatar = AvatarCards.FirstOrDefault(a =>
                string.Equals(a.Id, currentAvatarId ?? EMPTY, StringComparison.Ordinal));
        }

        private void Confirm()
        {
            if (SelectedAvatar == null)
            {
                alertService.Info(
                    localizationService.Get(KEY_UI_SELECT_REQUIRED),
                    localizationService.Get(KEY_UI_TITLE_INFO));
                return;
            }

            AvatarSelected?.Invoke(SelectedAvatar.Id);
            RequestClose?.Invoke();
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }

        private bool CanExecuteCommands() => !IsBusy;
    }

    public sealed class AvatarCardViewModel
    {
        private const string EMPTY = "";

        public AvatarCardViewModel(string id, string imagePath)
        {
            Id = id ?? EMPTY;
            ImagePath = imagePath ?? EMPTY;
        }

        public string Id { get; }
        public string ImagePath { get; }
    }
}
