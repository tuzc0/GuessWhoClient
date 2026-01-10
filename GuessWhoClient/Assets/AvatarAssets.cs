using System;
using System.Collections.Generic;

namespace GuessWhoClient.Assets
{
    public static class AvatarAssets
    {
        private const string ASSEMBLY_NAME = "GuessWhoClient";
        private const string PACK_BASE_URI = "pack://application:,,,/" + ASSEMBLY_NAME + ";component";
        private const string DEFAULT_AVATAR_ID = "A0001";

        private static readonly Dictionary<string, string> avatarById =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["A0001"] = PACK_BASE_URI + "/Images/Avatars/Avatar1.png",
                ["A0002"] = PACK_BASE_URI + "/Images/Avatars/Avatar2.png"
            };

        public static string GetDefaultAvatarId() => DEFAULT_AVATAR_ID;

        public static string GetAvatarUriById(string avatarId)
        {
            string safeId = (avatarId ?? string.Empty).Trim();

            if (avatarById.TryGetValue(safeId, out string uri) && !string.IsNullOrWhiteSpace(uri))
            {
                return uri;
            }

            return avatarById[DEFAULT_AVATAR_ID];
        }

        public static IReadOnlyDictionary<string, string> GetAllAvatars()
        {
            return avatarById;
        }
    }
}
