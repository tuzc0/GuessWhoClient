using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class UserAccountUiFaultMapper : IUiFaultMapper
    {
        private const string REQUESTNULL = "FaultInvalidRequest";

        private static readonly IReadOnlyDictionary<string, string> UserRegistrationMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { UserRegistrationFaultKeys.CODE_EMAIL_ALREADY_EXISTS, "FaultDuplicateEmail" },

                { UserRegistrationFaultKeys.CODE_REQUEST_NULL, REQUESTNULL },
                { UserRegistrationFaultKeys.CODE_ARGS_REQUIRED, REQUESTNULL },
                { UserRegistrationFaultKeys.CODE_VALIDATION_FAILED, "FaultInvalidRequest" },

                { UserRegistrationFaultKeys.CODE_EMAIL_REQUIRED, "UiValidationEmailRequired" },
                { UserRegistrationFaultKeys.CODE_EMAIL_INVALID, "UiValidationEmailFormat" },

                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_REQUIRED, "UiValidationDisplayNameRequired" },
                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_INVALID, "UiValidationDisplayNameInvalid" },

                { UserRegistrationFaultKeys.CODE_PASSWORD_REQUIRED, "UiValidationPasswordRequired" },
                { UserRegistrationFaultKeys.CODE_PASSWORD_INVALID, "UiValidationPasswordInvalid" },

                { UserRegistrationFaultKeys.CODE_UNEXPECTED_ERROR, "FaultUnexpected" }
            };

        private static readonly IReadOnlyDictionary<string, string> EmailVerificationMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { EmailVerificationFaultKeys.CODE_REQUEST_NULL, REQUESTNULL },

                { EmailVerificationFaultKeys.CODE_ACCOUNT_NOT_FOUND, "EmailVerificationAccountNotFound" },

                { EmailVerificationFaultKeys.CODE_CODE_MISSING, "UIVerificationCodeRequired" },
                { EmailVerificationFaultKeys.CODE_CODE_INVALID_FORMAT, "UiValidationSixDigits" },

                { EmailVerificationFaultKeys.CODE_CODE_INCORRECT, "UIVerificationCodeInvalid" },
                { EmailVerificationFaultKeys.CODE_CODE_ALREADY_USED, "UIVerificationCodeInvalid" },

                { EmailVerificationFaultKeys.CODE_CODE_EXPIRED, "UIVerificationCodeExpired" },

                { EmailVerificationFaultKeys.CODE_RESEND_TOO_FREQUENT, "EmailVerificationResendLimit" },
                { EmailVerificationFaultKeys.CODE_RESEND_HOURLY_LIMIT_EXCEEDED, "EmailVerificationResendLimit" },

                { EmailVerificationFaultKeys.CODE_EMAIL_VERIFICATION_FAILED, "EmailVerificationFailed" },

                { EmailVerificationFaultKeys.CODE_EMAIL_RECIPIENT_INVALID, "Infrastructure.Email.RecipientInvalid" },

                { EmailVerificationFaultKeys.CODE_SMTP_CONFIGURATION_MISSING, "Infrastructure.Email.ConfigurationMissing" },
                { EmailVerificationFaultKeys.CODE_SMTP_CONFIGURATION_ERROR, "Infrastructure.Email.ConfigurationError" },
                { EmailVerificationFaultKeys.CODE_SMTP_AUTHENTICATION_FAILED, "Infrastructure.Email.AuthenticationFailed" },
                { EmailVerificationFaultKeys.CODE_SMTP_UNAVAILABLE, "Infrastructure.Email.Unavailable" },
                { EmailVerificationFaultKeys.CODE_EMAIL_SEND_FAILED, "Infrastructure.Email.SendFailed" },

                { EmailVerificationFaultKeys.CODE_UNEXPECTED_ERROR, "FaultUnexpected" }
            };

        private static readonly IReadOnlyDictionary<string, string> PasswordRecoveryMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { PasswordRecoveryFaultKeys.CODE_REQUEST_NULL, REQUESTNULL },

                { PasswordRecoveryFaultKeys.CODE_ACCOUNT_NOT_FOUND, "PasswordRecoveryAccountNotFound" },

                { PasswordRecoveryFaultKeys.CODE_CODE_EXPIRED, "UIVerificationCodeExpired" },
                { PasswordRecoveryFaultKeys.CODE_CODE_INVALID, "UIVerificationCodeInvalid" },

                { PasswordRecoveryFaultKeys.CODE_UPDATE_PASSWORD_DB_FAILED, "PasswordRecoveryUpdateFailed" },

                { PasswordRecoveryFaultKeys.CODE_UNEXPECTED_ERROR, "FaultUnexpected" }
            };

        private static readonly IReadOnlyDictionary<string, string> InfrastructureEmailMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { InfrastructureEmailFaultKeys.CODE_EMAIL_RECIPIENT_INVALID, "Infrastructure.Email.RecipientInvalid" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_MISSING, "Infrastructure.Email.ConfigurationMissing" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_ERROR, "Infrastructure.Email.ConfigurationError" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_AUTHENTICATION_FAILED, "Infrastructure.Email.AuthenticationFailed" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_TIMEOUT, "Infrastructure.Email.Timeout" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNAVAILABLE, "Infrastructure.Email.Unavailable" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_SEND_FAILED, "Infrastructure.Email.SendFailed" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNEXPECTED_ERROR, "Infrastructure.Email.UnexpectedError" }
            };

        public UiKeyMapping Map(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return UiKeyMapping.Unmapped();
            }

            if (UserRegistrationMap.TryGetValue(faultCode, out string uiKey))
            {
                return UiKeyMapping.Mapped(uiKey);
            }

            if (EmailVerificationMap.TryGetValue(faultCode, out uiKey))
            {
                return UiKeyMapping.Mapped(uiKey);
            }

            if (PasswordRecoveryMap.TryGetValue(faultCode, out uiKey))
            {
                return UiKeyMapping.Mapped(uiKey);
            }

            if (InfrastructureEmailMap.TryGetValue(faultCode, out uiKey))
            {
                return UiKeyMapping.Mapped(uiKey);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
