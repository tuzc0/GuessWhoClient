using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class InfrastructureEmailFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { InfrastructureEmailFaultKeys.CODE_EMAIL_RECIPIENT_INVALID, "UiEmail.RecipientInvalid" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_MISSING, "UiEmail.ConfigurationMissing" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_CONFIGURATION_ERROR, "UiEmail.ConfigurationError" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_AUTHENTICATION_FAILED, "UiEmail.AuthenticationFailed" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_TIMEOUT, "UiEmail.Timeout" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNAVAILABLE, "UiEmail.Unavailable" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_SEND_FAILED, "UiEmail.SendFailed" },
                { InfrastructureEmailFaultKeys.CODE_EMAIL_UNEXPECTED_ERROR, "UiEmail.UnexpectedError" },
            };

        public string ResolveUiKey(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return EMPTY;
            }

            return MapTable.TryGetValue(faultCode, out string key)
                ? key ?? EMPTY
                : EMPTY;
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
