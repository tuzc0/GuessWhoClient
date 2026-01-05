using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class PasswordRecoveryUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_ACCOUNT_NOT_FOUND = "PasswordRecoveryAccountNotFound";
        private const string KEY_CODE_EXPIRED = "PasswordRecoveryCodeExpired";
        private const string KEY_CODE_INVALID = "PasswordRecoveryCodeInvalid";
        private const string KEY_UPDATE_FAILED = "PasswordRecoveryUpdateFailed";
        private const string KEY_UNEXPECTED = "UiGenericError";

        public UiKeyMapping Map(string faultCode)
        {
            if (string.Equals(faultCode, PasswordRecoveryFaultKeys.CODE_ACCOUNT_NOT_FOUND, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_ACCOUNT_NOT_FOUND);
            }

            if (string.Equals(faultCode, PasswordRecoveryFaultKeys.CODE_CODE_EXPIRED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_CODE_EXPIRED);
            }

            if (string.Equals(faultCode, PasswordRecoveryFaultKeys.CODE_CODE_INVALID, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_CODE_INVALID);
            }

            if (string.Equals(faultCode, PasswordRecoveryFaultKeys.CODE_UPDATE_PASSWORD_DB_FAILED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UPDATE_FAILED);
            }

            if (string.Equals(faultCode, PasswordRecoveryFaultKeys.CODE_UNEXPECTED_ERROR, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UNEXPECTED);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
