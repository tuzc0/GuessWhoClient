using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System;

namespace GuessWhoClient.Application.ErrorHandling.Auth
{
    public sealed class LoginUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_INVALID_CREDENTIALS = "LoginInvalidCredentials";
        private const string KEY_ACCOUNT_LOCKED = "LoginAccountLocked";

        public bool TryMap(string faultCode, out string uiKey)
        {
            uiKey = null;

            if (string.Equals(faultCode, LoginFaultKeys.CODE_INVALID_CREDENTIALS, StringComparison.Ordinal))
            {
                uiKey = KEY_INVALID_CREDENTIALS;
                return true;
            }

            if (string.Equals(faultCode, LoginFaultKeys.CODE_ACCOUNT_LOCKED, StringComparison.Ordinal))
            {
                uiKey = KEY_ACCOUNT_LOCKED;
                return true;
            }

            return false;
        }
    }

}
