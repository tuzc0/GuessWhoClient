using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GuessWhoClient.Presentation.ViewsModels.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private bool isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if(SetProperty(ref isBusy, value))
                {
                    OnIsBusyChanged(nameof(IsBusy));
                }
            }
        }

        protected virtual void OnIsBusyChanged(string propertyName)
        {
        }

        protected bool SetProperty<T>(ref T storage,  T value,
            [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
