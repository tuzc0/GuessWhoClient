using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class InfrastructureEmailFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_RECIPIENT_INVALID = "UiInfrastructureEmailRecipientInvalid";
        private const string KEY_CONFIGURATION_MISSING = "UiInfrastructureEmailConfigurationMissing";
        private const string KEY_CONFIGURATION_ERROR = "UiInfrastructureEmailConfigurationError";
        private const string KEY_AUTH_FAILED = "UiInfrastructureEmailAuthenticationFailed";
        private const string KEY_TIMEOUT = "UiInfrastructureEmailTimeout";
        private const string KEY_UNAVAILABLE = "UiInfrastructureEmailUnavailable";
        private const string KEY_SEND_FAILED = "UiInfrastructureEmailSendFailed";
        private const string KEY_UNEXPECTED = "UiInfrastructureEmailUnexpectedError";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { InfrastructureEmailFaultKeys.CODE_EMAIL_RECIPIENT_INVALID, KEY_RECIPIENT_INVALID },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_MISSING, KEY_CONFIGURATION_MISSING },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_ERROR, KEY_CONFIGURATION_ERROR },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_AUTHENTICATION_FAILED, KEY_AUTH_FAILED },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_TIMEOUT, KEY_TIMEOUT },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNAVAILABLE, KEY_UNAVAILABLE },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_SEND_FAILED, KEY_SEND_FAILED },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNEXPECTED_ERROR, KEY_UNEXPECTED },
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
