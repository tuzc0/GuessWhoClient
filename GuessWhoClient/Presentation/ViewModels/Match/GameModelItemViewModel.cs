using GuessWhoClient.Presentation.ViewsModels.Base;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class GameModeItemViewModel : ViewModelBase
    {
        private bool isSelected;
        private bool isEnabled;

        public GameModeItemViewModel(byte id, string name)
        {
            Id = id;
            Name = name ?? string.Empty;
            IsEnabled = true;
        }

        public byte Id { get; }
        public string Name { get; }

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
        }
    }
}
