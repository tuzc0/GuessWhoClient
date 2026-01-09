using GuessWhoClient.Infraestructure.Wcf;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class WcfUiFaultMapper : IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_ENDPOINT_NOT_FOUND = "UiCommunicationEndpointNotFound";
        private const string KEY_COMMUNICATION_ERROR = "UiCommunicationError";
        private const string KEY_SERVICE_UNAVAILABLE = "UiCommunicationServiceUnavailable";
        private const string KEY_SECURITY_ERROR = "UiCommunicationSecurityError";
        private const string KEY_TIMEOUT = "UiCommunicationTimeout";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, KEY_ENDPOINT_NOT_FOUND },
                { WcfTechnicalFaultCodes.COMMUNICATION_ERROR, KEY_COMMUNICATION_ERROR },
                { WcfTechnicalFaultCodes.SECURITY_ERROR, KEY_SECURITY_ERROR },
                { WcfTechnicalFaultCodes.TIMEOUT, KEY_TIMEOUT },
            };

        public UiKeyMapping Map(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return UiKeyMapping.Unmapped();
            }

            if (MapTable.ContainsKey(faultCode))
            {
                string uiKey = MapTable[faultCode] ?? EMPTY;

                return string.IsNullOrWhiteSpace(uiKey)
                    ? UiKeyMapping.Unmapped()
                    : UiKeyMapping.Mapped(uiKey);
            }

            return UiKeyMapping.Mapped(KEY_SERVICE_UNAVAILABLE);
        }
    }
}
