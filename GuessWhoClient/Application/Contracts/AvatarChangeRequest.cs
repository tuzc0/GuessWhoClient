namespace GuessWhoClient.Dtos
{
    public sealed class AvatarChangeRequest
    {
        public string AvatarId { get; }

        public AvatarChangeRequest(string avatarId)
        {
            AvatarId = (avatarId ?? string.Empty).Trim();
        }
    }
}
