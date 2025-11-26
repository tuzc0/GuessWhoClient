using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GuessWhoClient.Assets;
using GuessWhoClient.Dtos;

namespace GuessWhoClient
{
    public partial class ChooseAvatarWindow : UserControl, INotifyPropertyChanged
    {
        private const string VALIDATION_TITLE = "Selection";
        private const string ERROR_AVATAR_NOT_SELECTED = "Please select an avatar first.";

        public ObservableCollection<AvatarCard> AvatarCards { get; } =
            new ObservableCollection<AvatarCard>();

        private AvatarCard selectedAvatarCard;
        private bool hasSelection;

        public string SelectedAvatarId { get; private set; }
        public AvatarChangeRequest AvatarChangeRequest { get; private set; }

        public event EventHandler<bool> AvatarChangeClose;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasSelection
        {
            get => hasSelection;
            set
            {
                if (hasSelection == value)
                {
                    return;
                }

                hasSelection = value;
                OnPropertyChanged();
            }
        }

        public ChooseAvatarWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadAvatars();
        }

        private void LoadAvatars()
        {
            AvatarCards.Clear();

            var avatars = AvatarAssets.GetAllAvatars();

            foreach (var avatar in avatars)
            {
                AvatarCards.Add(new AvatarCard
                {
                    Id = avatar.Key,
                    ImagePath = avatar.Value
                });
            }
        }

        private void CharacterCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AvatarCard clickedCard)
            {
                foreach (var card in AvatarCards)
                {
                    card.IsSelected = ReferenceEquals(card, clickedCard);
                }

                selectedAvatarCard = clickedCard;
                HasSelection = selectedAvatarCard != null;
            }
        }

        private void SelectAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAvatarCard == null)
            {
                MessageBox.Show(
                    ERROR_AVATAR_NOT_SELECTED,
                    VALIDATION_TITLE,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                AvatarChangeRequest = null;
                return;
            }

            SelectedAvatarId = selectedAvatarCard.Id;

            AvatarChangeRequest = new AvatarChangeRequest(SelectedAvatarId);

            AvatarChangeClose?.Invoke(this, true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAvatarId = null;
            AvatarChangeRequest = null;
            AvatarChangeClose?.Invoke(this, false);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
