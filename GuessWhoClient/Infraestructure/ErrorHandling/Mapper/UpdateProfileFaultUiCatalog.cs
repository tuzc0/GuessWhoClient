using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class UpdateProfileFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { UpdateProfileFaultKeys.CODE_REQUEST_NULL, "UiProfileRequestNull" },
                { UpdateProfileFaultKeys.CODE_USER_ID_INVALID, "UiSessionInvalidOrExpired" },
                { UpdateProfileFaultKeys.CODE_PROFILE_NOT_FOUND, "UiProfileNotFound" },

                { UpdateProfileFaultKeys.CODE_NO_CHANGES_PROVIDED, "UiProfileNoChangesProvided" },

                { UpdateProfileFaultKeys.CODE_CURRENT_PASSWORD_REQUIRED, "UiProfileCurrentPasswordRequired" },
                { UpdateProfileFaultKeys.CODE_CURRENT_PASSWORD_INCORRECT, "UiProfileCurrentPasswordIncorrect" },

                { UpdateProfileFaultKeys.CODE_DISPLAYNAME_INVALID, "UiProfileDisplayNameInvalid" },
                { UpdateProfileFaultKeys.CODE_AVATAR_INVALID, "UiProfileAvatarInvalid" },
                { UpdateProfileFaultKeys.CODE_PASSWORD_INVALID, "UiProfilePasswordInvalid" },

                { UpdateProfileFaultKeys.CODE_UPDATE_FAILED, "UiProfileUpdateFailed" },
                { UpdateProfileFaultKeys.CODE_PROFILE_DELETE_FAILED, "UiProfileDeleteFailed" },
            };

        public string ResolveUiKey(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return EMPTY;
            }

            if (!MapTable.ContainsKey(faultCode))
            {
                return EMPTY;
            }

            string key = MapTable[faultCode];
            return key ?? EMPTY;
        }

        public UiKeyMapping Map(string faultCode)
        {
            string uiKey = ResolveUiKey(faultCode);

            return string.IsNullOrWhiteSpace(uiKey)
                ? UiKeyMapping.Unmapped()
                : UiKeyMapping.Mapped(uiKey);
        }
    }
}
