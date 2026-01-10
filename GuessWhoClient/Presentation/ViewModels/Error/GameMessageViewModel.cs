using GuessWhoClient.Presentation.ViewsModels.Base;

namespace GuessWhoClient.Presentation.ViewModels.Error
{
    public sealed class GameMessageViewModel : ViewModelBase
    {
        private const string EMPTY = "";
        private const string DEFAULT_OK = "OK";

        private string messageTitle;
        private string messageText;

        public GameMessageViewModel(string title, string message)
            : this(title, message, DEFAULT_OK, EMPTY)
        {
        }

        public GameMessageViewModel(string title, string message, string primaryButtonText, string secondaryButtonText)
        {
            messageTitle = title ?? EMPTY;
            messageText = message ?? EMPTY;

            PrimaryButtonText = string.IsNullOrWhiteSpace(primaryButtonText) ? DEFAULT_OK : primaryButtonText;
            SecondaryButtonText = secondaryButtonText ?? EMPTY;
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

        public string PrimaryButtonText { get; }
        public string SecondaryButtonText { get; }

        public bool HasSecondaryButton => !string.IsNullOrWhiteSpace(SecondaryButtonText);
    }
}
