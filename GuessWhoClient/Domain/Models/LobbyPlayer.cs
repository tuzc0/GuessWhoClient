using GuessWhoClient.Assets;

namespace GuessWhoClient.Domain.Models
{
    public sealed class LobbyPlayer
    {
        public LobbyPlayer(
            long matchId,
            long userId,
            string displayName,
            string avatarId,
            int slotNumber,
            bool isReady,
            bool isHost)
        {
            MatchId = matchId;
            UserId = userId;
            DisplayName = displayName ?? string.Empty;
            AvatarId = avatarId ?? string.Empty;
            SlotNumber = slotNumber;
            IsReady = isReady;
            IsHost = isHost;
        }

        public long MatchId { get; }
        public long UserId { get; }
        public string DisplayName { get; }
        public string AvatarId { get; }
        public int SlotNumber { get; }
        public bool IsReady { get; }
        public bool IsHost { get; }

        public string AvatarPath
        {
            get
            {
                return AvatarAssets.GetAvatarPathById(AvatarId);
            }
        }
    }
}
