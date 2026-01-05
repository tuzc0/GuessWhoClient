using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class EmailVerificationUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_ACCOUNT_NOT_FOUND = "EmailVerificationAccountNotFound";
        private const string KEY_CODE_MISSING = "EmailVerificationCodeMissing";
        private const string KEY_CODE_EXPIRED = "EmailVerificationCodeExpired";
        private const string KEY_CODE_ALREADY_USED = "EmailVerificationCodeAlreadyUsed";
        private const string KEY_VERIFICATION_FAILED = "EmailVerificationFailed";
        private const string KEY_RESEND_LIMIT = "EmailVerificationResendLimit";
        private const string KEY_UNEXPECTED = "UiGenericError";

        public UiKeyMapping Map(string faultCode)
        {
            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_ACCOUNT_NOT_FOUND, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_ACCOUNT_NOT_FOUND);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_CODE_MISSING, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_CODE_MISSING);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_CODE_EXPIRED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_CODE_EXPIRED);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_CODE_ALREADY_USED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_CODE_ALREADY_USED);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_EMAIL_VERIFICATION_FAILED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_VERIFICATION_FAILED);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_RESEND_HOURLY_LIMIT_EXCEEDED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_RESEND_LIMIT);
            }

            if (string.Equals(faultCode, EmailVerificationFaultKeys.CODE_UNEXPECTED_ERROR, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UNEXPECTED);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
