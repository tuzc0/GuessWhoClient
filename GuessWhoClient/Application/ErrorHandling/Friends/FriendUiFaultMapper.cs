using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System.Collections.Generic;

namespace GuessWhoClient.Application.ErrorHandling.Friends
{
    public sealed class FriendUiFaultMapper : IUiFaultMapper
    {
        private static readonly HashSet<string> KnownFaults = new HashSet<string>
        {
            FriendFaultKeys.CODE_REQUEST_NULL,
            FriendFaultKeys.CODE_INVALID_ACCOUNT_ID,
            FriendFaultKeys.CODE_ACCOUNT_NOT_FOUND,
            FriendFaultKeys.CODE_INVALID_DISPLAY_NAME,
            FriendFaultKeys.CODE_INVALID_IDS,
            FriendFaultKeys.CODE_CANNOT_FRIEND_SELF,
            FriendFaultKeys.CODE_DESTINATION_INACTIVE,
            FriendFaultKeys.CODE_NOT_AUTHORIZED,
            FriendFaultKeys.CODE_NOT_FOUND,
            FriendFaultKeys.CODE_NOT_PENDING,
            FriendFaultKeys.CODE_ALREADY_FRIENDS,
            FriendFaultKeys.CODE_REQUEST_ID_NOT_GENERATED,
            FriendFaultKeys.CODE_REQUEST_ALREADY_PENDING
        };

        public UiKeyMapping Map(string faultCode)
        {
            if (faultCode != null && KnownFaults.Contains(faultCode))
            {
                return new UiKeyMapping(true, faultCode);
            }

            return new UiKeyMapping(false, null);
        }
    }
}