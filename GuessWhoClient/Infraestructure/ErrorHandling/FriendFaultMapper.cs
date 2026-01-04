using System;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class FriendUiFaultMapper : IUiFaultMapper
    {
        public bool TryMap(string faultCode, out string uiKey)
        {
            uiKey = null;

            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return false;
            }

            uiKey = faultCode;

            return true;
        }
    }
}