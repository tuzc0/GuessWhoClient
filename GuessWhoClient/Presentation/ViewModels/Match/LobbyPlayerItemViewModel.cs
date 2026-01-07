using GuessWhoClient.Presentation.ViewsModels.Base;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class LobbyPlayerItemViewModel : ViewModelBase
    {
        private long userId;
        private string displayName;
        private string avatarPath;
        private bool isReady;
        private bool isHost;

        public long UserId
        {
            get => userId;
            set => SetProperty(ref userId, value);
        }

        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        public string AvatarPath
        {
            get => avatarPath;
            set => SetProperty(ref avatarPath, value);
        }

        public bool IsReady
        {
            get => isReady;
            set => SetProperty(ref isReady, value);
        }

        public bool IsHost
        {
            get => isHost;
            set => SetProperty(ref isHost, value);
        }
    }
}
