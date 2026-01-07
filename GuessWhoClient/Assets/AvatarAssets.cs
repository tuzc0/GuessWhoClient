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
            const string defaultPath = "/Images/Avatars/Avatar1.png";

            if (string.IsNullOrWhiteSpace(avatarId))
            {
                return defaultPath;
            }

            if (avatarById.TryGetValue(avatarId, out string path) && !string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return defaultPath;
        }

        public static IReadOnlyDictionary<string, string> GetAllAvatars()
        {
            return avatarById;
        }
    }
}
