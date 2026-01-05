using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System.Collections.Generic;

namespace GuessWhoClient.Application.ErrorHandling.Profile
{
    public sealed class ProfileUiFaultMapper : IUiFaultMapper
    {
        private static readonly HashSet<string> KnownFaults = new HashSet<string>
        {
            UpdateProfileFaultKeys.CODE_REQUEST_NULL,
            UpdateProfileFaultKeys.CODE_USER_ID_INVALID,
            UpdateProfileFaultKeys.CODE_PROFILE_NOT_FOUND,
            UpdateProfileFaultKeys.CODE_NO_CHANGES_PROVIDED,
            UpdateProfileFaultKeys.CODE_CURRENT_PASSWORD_REQUIRED,
            UpdateProfileFaultKeys.CODE_CURRENT_PASSWORD_INCORRECT,
            UpdateProfileFaultKeys.CODE_UPDATE_FAILED,
            UpdateProfileFaultKeys.CODE_PROFILE_DELETE_FAILED,
            UpdateProfileFaultKeys.CODE_DISPLAYNAME_INVALID,
            UpdateProfileFaultKeys.CODE_AVATAR_INVALID,
            UpdateProfileFaultKeys.CODE_PASSWORD_INVALID
        };

        public UiKeyMapping Map(string faultCode)
        {
            if (faultCode != null && KnownFaults.Contains(faultCode))
            {
                return new UiKeyMapping(true, faultCode);
            }

            return new UiKeyMapping(false, null);
        }
    }
}