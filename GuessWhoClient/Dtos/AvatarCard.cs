using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GuessWhoClient.Dtos
{
    public sealed class AvatarCard : INotifyPropertyChanged
    {
        private bool isSelected; 
        public string Id { get; set; }
        public string ImagePath { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if(isSelected == value)
                {
                    return;
                }

                isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
