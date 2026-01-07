using GuessWhoClient.Presentation.ViewsModels.Base;

namespace GuessWhoClient.Presentation.ViewModels.Error
{
    public sealed class GameMessageViewModel : ViewModelBase
    {
        private const string EMPTY = "";

        private string messageTitle = EMPTY;
        private string messageText = EMPTY;

        public GameMessageViewModel(string title, string text)
        {
            MessageTitle = title ?? EMPTY;
            MessageText = text ?? EMPTY;
        }

        public string MessageTitle
        {
            get => messageTitle; 
            set => SetProperty(ref messageTitle, value ?? EMPTY);
        }

        public string MessageText
        {
            get => messageText; 
            set => SetProperty(ref messageText, value ?? EMPTY);
        }
    }
}
