using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class EmailVerificationFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_REQUEST_NULL = "UiEmailVerificationRequestNull";
        private const string KEY_UNEXPECTED = "UiEmailVerificationUnexpectedError";
        private const string KEY_ACCOUNT_NOT_FOUND = "UiEmailVerificationAccountNotFound";

        private const string KEY_CODE_MISSING = "UiEmailVerificationCodeMissing";
        private const string KEY_CODE_INVALID_FORMAT = "UiEmailVerificationCodeInvalidFormat";
        private const string KEY_CODE_INCORRECT = "UiEmailVerificationCodeIncorrect";
        private const string KEY_CODE_EXPIRED = "UiEmailVerificationCodeExpired";
        private const string KEY_CODE_ALREADY_USED = "UiEmailVerificationCodeAlreadyUsed";

        private const string KEY_RESEND_TOO_FREQUENT = "UiEmailVerificationResendTooFrequent";
        private const string KEY_RESEND_HOURLY_LIMIT = "UiEmailVerificationResendHourlyLimitExceeded";

        private const string KEY_VERIFICATION_FAILED = "UiEmailVerificationVerificationFailed";
        private const string KEY_TOKEN_CREATION_FAILED = "UiEmailVerificationTokenCreationFailed";

        private const string KEY_CRYPTO_RANDOM_UNAVAILABLE = "UiEmailVerificationCryptoRandomUnavailable";
        private const string KEY_CODE_GENERATION_FAILED = "UiEmailVerificationCodeGenerationFailed";

        // Email component (server) - NO es InfraEmail
        private const string KEY_EMAIL_RECIPIENT_INVALID = "UiEmailRecipientInvalid";
        private const string KEY_EMAIL_CONFIG_MISSING = "UiEmailConfigurationMissing";
        private const string KEY_EMAIL_AUTH_FAILED = "UiEmailAuthenticationFailed";
        private const string KEY_EMAIL_CONFIG_ERROR = "UiEmailConfigurationError";
        private const string KEY_EMAIL_UNAVAILABLE = "UiEmailUnavailable";
        private const string KEY_EMAIL_SEND_FAILED = "UiEmailSendFailed";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { EmailVerificationFaultKeys.CODE_REQUEST_NULL, KEY_REQUEST_NULL },
                { EmailVerificationFaultKeys.CODE_UNEXPECTED_ERROR, KEY_UNEXPECTED },
                { EmailVerificationFaultKeys.CODE_ACCOUNT_NOT_FOUND, KEY_ACCOUNT_NOT_FOUND },

                { EmailVerificationFaultKeys.CODE_CODE_MISSING, KEY_CODE_MISSING },
                { EmailVerificationFaultKeys.CODE_CODE_INVALID_FORMAT, KEY_CODE_INVALID_FORMAT },
                { EmailVerificationFaultKeys.CODE_CODE_INCORRECT, KEY_CODE_INCORRECT },
                { EmailVerificationFaultKeys.CODE_CODE_EXPIRED, KEY_CODE_EXPIRED },
                { EmailVerificationFaultKeys.CODE_CODE_ALREADY_USED, KEY_CODE_ALREADY_USED },

                { EmailVerificationFaultKeys.CODE_RESEND_TOO_FREQUENT, KEY_RESEND_TOO_FREQUENT },
                { EmailVerificationFaultKeys.CODE_RESEND_HOURLY_LIMIT_EXCEEDED, KEY_RESEND_HOURLY_LIMIT },

                { EmailVerificationFaultKeys.CODE_EMAIL_VERIFICATION_FAILED, KEY_VERIFICATION_FAILED },
                { EmailVerificationFaultKeys.CODE_TOKEN_CREATION_FAILED, KEY_TOKEN_CREATION_FAILED },

                { EmailVerificationFaultKeys.CODE_CRYPTO_RANDOM_GENERATOR_UNAVAILABLE, KEY_CRYPTO_RANDOM_UNAVAILABLE },
                { EmailVerificationFaultKeys.CODE_VERIFICATION_CODE_GENERATION_FAILED, KEY_CODE_GENERATION_FAILED },

                { EmailVerificationFaultKeys.CODE_EMAIL_RECIPIENT_INVALID, KEY_EMAIL_RECIPIENT_INVALID },
                { EmailVerificationFaultKeys.CODE_SMTP_CONFIGURATION_MISSING, KEY_EMAIL_CONFIG_MISSING },
                { EmailVerificationFaultKeys.CODE_SMTP_AUTHENTICATION_FAILED, KEY_EMAIL_AUTH_FAILED },
                { EmailVerificationFaultKeys.CODE_SMTP_CONFIGURATION_ERROR, KEY_EMAIL_CONFIG_ERROR },
                { EmailVerificationFaultKeys.CODE_SMTP_UNAVAILABLE, KEY_EMAIL_UNAVAILABLE },
                { EmailVerificationFaultKeys.CODE_EMAIL_SEND_FAILED, KEY_EMAIL_SEND_FAILED },
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
