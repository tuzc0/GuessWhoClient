using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class FriendFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FriendFaultKeys.CODE_REQUEST_NULL, "UiFriendRequestNull" },
                { FriendFaultKeys.CODE_INVALID_ACCOUNT_ID, "UiFriendInvalidAccountId" },
                { FriendFaultKeys.CODE_ACCOUNT_NOT_FOUND, "UiFriendAccountNotFound" },
                { FriendFaultKeys.CODE_INVALID_DISPLAY_NAME, "UiFriendInvalidDisplayName" },
                { FriendFaultKeys.CODE_INVALID_IDS, "UiFriendInvalidIds" },
                { FriendFaultKeys.CODE_CANNOT_FRIEND_SELF, "UiFriendCannotFriendSelf" },
                { FriendFaultKeys.CODE_DESTINATION_INACTIVE, "UiFriendDestinationInactive" },
                { FriendFaultKeys.CODE_NOT_AUTHORIZED, "UiFriendNotAuthorized" },
                { FriendFaultKeys.CODE_NOT_FOUND, "UiFriendRequestNotFound" },
                { FriendFaultKeys.CODE_NOT_PENDING, "UiFriendRequestNotPending" },
                { FriendFaultKeys.CODE_ALREADY_FRIENDS, "UiFriendAlreadyFriends" },
                { FriendFaultKeys.CODE_REQUEST_ID_NOT_GENERATED, "UiFriendRequestIdNotGenerated" },
                { FriendFaultKeys.CODE_REQUEST_ALREADY_PENDING, "UiFriendRequestAlreadyPending" },
                { FriendFaultKeys.CODE_UNEXPECTED_ERROR, "UiFriendUnexpectedError" },
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
