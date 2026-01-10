namespace GuessWhoClient.Assets
{
    public sealed class AvatarPathResolver : IAvatarPathResolver
    {
        public string Resolve(string avatarId)
        {
            return AvatarAssets.GetAvatarUriById(avatarId);
        }
    }
}
