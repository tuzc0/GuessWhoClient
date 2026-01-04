using System;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class WcfUiFaultMapper : IUiFaultMapper
    {
        private const string CODE_ENDPOINT_NOT_FOUND = "WCF_ENDPOINT_NOT_FOUND";
        private const string CODE_SECURITY_ERROR = "WCF_SECURITY_ERROR";
        private const string CODE_TIMEOUT = "WCF_TIMEOUT";
        private const string CODE_COMMUNICATION = "WCF_COMMUNICATION_ERROR";

        private const string KEY_SERVICE_UNAVAILABLE = "UiServiceUnavailable";
        private const string KEY_SECURITY_ERROR = "UiSecurityError";
        private const string KEY_TIMEOUT = "UiTimeout";

        public bool TryMap(string faultCode, out string uiKey)
        {
            uiKey = null;

            if (string.Equals(faultCode, CODE_ENDPOINT_NOT_FOUND, StringComparison.Ordinal))
            {
                uiKey = KEY_SERVICE_UNAVAILABLE;
                return true;
            }

            if (string.Equals(faultCode, CODE_SECURITY_ERROR, StringComparison.Ordinal))
            {
                uiKey = KEY_SECURITY_ERROR;
                return true;
            }

            if (string.Equals(faultCode, CODE_TIMEOUT, StringComparison.Ordinal))
            {
                uiKey = KEY_TIMEOUT;
                return true;
            }

            if (string.Equals(faultCode, CODE_COMMUNICATION, StringComparison.Ordinal))
            {
                uiKey = KEY_SERVICE_UNAVAILABLE;
                return true;
            }

            return false;
        }
    }
}
