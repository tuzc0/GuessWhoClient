using GuessWhoClient.Assets;

namespace GuessWhoClient.Dtos
{
    public sealed class ClientLobbyPlayerDto
    {
        public long MatchId { get; set; }
        public long UserId { get; set; }
        public string DisplayName { get; set; }

        public string Avatar { get; set; }
        public byte SlotNumber { get; set; }
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }

        public string AvatarPath
        {
            get
            {
                return AvatarAssets.GetAvatarPathById(Avatar);
            }
        }
    }
}
