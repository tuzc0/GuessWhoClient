using GuessWhoClient.Infraestructure.Wcf;
using System;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class WcfUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_SERVICE_UNAVAILABLE = "UiServiceUnavailable";
        private const string KEY_SECURITY_ERROR = "UiSecurityError";
        private const string KEY_TIMEOUT = "UiTimeout";

        public UiKeyMapping Map(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return UiKeyMapping.Unmapped();
            }

            if (string.Equals(faultCode, WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_SERVICE_UNAVAILABLE);
            }

            if (string.Equals(faultCode, WcfTechnicalFaultCodes.SECURITY_ERROR, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_SECURITY_ERROR);
            }

            if (string.Equals(faultCode, WcfTechnicalFaultCodes.TIMEOUT, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_TIMEOUT);
            }

            if (string.Equals(faultCode, WcfTechnicalFaultCodes.COMMUNICATION_ERROR, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_SERVICE_UNAVAILABLE);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
