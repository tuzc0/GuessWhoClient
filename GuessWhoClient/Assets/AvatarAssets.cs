using System.Collections.Generic;

namespace GuessWhoClient.Assets
{
    public static class AvatarAssets
    {

        private static readonly Dictionary<string, string> avatarById = new Dictionary<string, string>
        {
            ["A0001"] = "/Images/Avatars/Avatar1.png",
            ["A0002"] = "/Images/Avatars/Avatar2.png",
        };

        public static string GetAvatarPathById(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return "/Images/Avatars/Avatar1.png";
            }

            if (avatarById.TryGetValue(avatarId, out var path))
            {
                return path;
            }

            return null;
        }
    }
}
