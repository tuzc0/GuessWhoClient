using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class LoginUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_UI_INVALID_CREDENTIALS = "LoginInvalidCredentials";
        private const string KEY_UI_ACCOUNT_LOCKED = "LoginAccountLocked";
        private const string KEY_UI_PROFILE_MARK_ACTIVE_FAILED = "LoginProfileActivationFailed";

        public UiKeyMapping Map(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return UiKeyMapping.Unmapped();
            }

            if (string.Equals(faultCode, LoginFaultKeys.CODE_INVALID_CREDENTIALS, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_INVALID_CREDENTIALS);
            }

            if (string.Equals(faultCode, LoginFaultKeys.CODE_ACCOUNT_LOCKED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_ACCOUNT_LOCKED);
            }

            if (string.Equals(faultCode, LoginCoordinatorFaultKeys.CODE_PROFILE_MARK_ACTIVE_FAILED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_PROFILE_MARK_ACTIVE_FAILED);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
