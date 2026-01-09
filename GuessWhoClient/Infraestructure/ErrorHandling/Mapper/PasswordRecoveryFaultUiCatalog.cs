using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class PasswordRecoveryFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { PasswordRecoveryFaultKeys.CODE_REQUEST_NULL, "UiPasswordRecoveryRequestNull" },
                { PasswordRecoveryFaultKeys.CODE_UNEXPECTED_ERROR, "UiPasswordRecoveryUnexpectedError" },

                { PasswordRecoveryFaultKeys.CODE_ACCOUNT_NOT_FOUND, "UiPasswordRecoveryAccountNotFound" },

                { PasswordRecoveryFaultKeys.CODE_RESEND_TOO_FREQUENT, "UiPasswordRecoveryResendTooFrequent" },
                { PasswordRecoveryFaultKeys.CODE_RESEND_HOURLY_LIMIT_EXCEEDED, "UiPasswordRecoveryResendHourlyLimitExceeded" },

                { PasswordRecoveryFaultKeys.CODE_CODE_EXPIRED, "UiPasswordRecoveryCodeExpired" },
                { PasswordRecoveryFaultKeys.CODE_CODE_INVALID, "UiPasswordRecoveryCodeInvalid" },

                { PasswordRecoveryFaultKeys.CODE_UPDATE_PASSWORD_DB_FAILED, "UiPasswordRecoveryUpdatePasswordFailed" },
                { PasswordRecoveryFaultKeys.CODE_TOKEN_CREATION_FAILED, "UiPasswordRecoveryTokenCreationFailed" },
                { PasswordRecoveryFaultKeys.CODE_EMAIL_MESSAGE_BUILD_FAILED, "UiPasswordRecoveryEmailMessageBuildFailed" },
                { PasswordRecoveryFaultKeys.CODE_EMAIL_SENDER_RETURNED_NULL, "UiPasswordRecoveryEmailSenderUnavailable" },
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
