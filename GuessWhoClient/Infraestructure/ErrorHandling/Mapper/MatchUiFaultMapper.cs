using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class MatchUiFaultMapper : IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string CODE_CREATE_INVALID_ARGS = "INVALID_ARGS";

        private const string CODE_JOIN_INVALID_ARGS_A = "MATCH_INVALID_ARGS";
        private const string CODE_JOIN_MATCH_NOT_JOINABLE = "MatchNotJoinable";
        private const string CODE_JOIN_PLAYER_ALREADY_IN_MATCH = "PlayerAlreadyInMatch";
        private const string CODE_JOIN_GUEST_SLOT_TAKEN = "GuestSlotTaken";
        private const string CODE_JOIN_IN_OTHER_ACTIVE_MATCH = "InOtherActiveMatch";
        private const string CODE_JOIN_OPERATION_CONFLICT = "OperationConflict";

        private const string CODE_SHARED_INVALID_ARGS = "InvalidArgs";
        private const string CODE_SHARED_MATCH_NOT_FOUND = "MatchNotFound";
        private const string CODE_SHARED_MATCH_NOT_IN_LOBBY = "MatchNotInLobby";
        private const string CODE_SHARED_HOST_NOT_AUTHORIZED = "HostNotAuthorized";
        private const string CODE_SHARED_CONCURRENT_UPDATE = "ConcurrentUpdate";
        private const string CODE_SHARED_UNEXPECTED_ERROR = "UnexpectedError";

        private const string CODE_VIS_ALREADY_DESIRED = "AlreadyInDesiredVisibility";
        private const string CODE_VIS_ALREADY_PRIVATE = "AlreadyPrivate";
        private const string CODE_VIS_ALREADY_PUBLIC = "AlreadyPublic";
    
        private const string CODE_START_NOT_ENOUGH_PLAYERS = "NotEnoughPlayers";
        private const string CODE_START_PLAYERS_NOT_READY = "PlayersNotReady";

        private const string KEY_CREATE_INVALID_ARGS = "UiMatchCreateInvalidArgs";

        private const string KEY_JOIN_INVALID_ARGS = "UiMatchJoinInvalidArgs";
        private const string KEY_JOIN_MATCH_NOT_JOINABLE = "UiMatchJoinMatchNotJoinable";
        private const string KEY_JOIN_ALREADY_IN_MATCH = "UiMatchJoinPlayerAlreadyInMatch";
        private const string KEY_JOIN_GUEST_SLOT_TAKEN = "UiMatchJoinGuestSlotTaken";
        private const string KEY_JOIN_IN_OTHER_ACTIVE_MATCH = "UiMatchJoinInOtherActiveMatch";
        private const string KEY_JOIN_OPERATION_CONFLICT = "UiMatchJoinOperationConflict";

        private const string KEY_SHARED_INVALID_ARGS = "UiMatchInvalidArgs";
        private const string KEY_SHARED_MATCH_NOT_FOUND = "UiMatchMatchNotFound";
        private const string KEY_SHARED_MATCH_NOT_IN_LOBBY = "UiMatchMatchNotInLobby";
        private const string KEY_SHARED_HOST_NOT_AUTHORIZED = "UiMatchHostNotAuthorized";
        private const string KEY_SHARED_CONCURRENT_UPDATE = "UiMatchConcurrentUpdate";
        private const string KEY_SHARED_UNEXPECTED_ERROR = "UiMatchUnexpectedError";

        private const string KEY_VIS_ALREADY_DESIRED = "UiMatchVisibilityAlreadyInDesiredVisibility";

        private const string KEY_START_NOT_ENOUGH_PLAYERS = "UiMatchStartNotEnoughPlayers";
        private const string KEY_START_PLAYERS_NOT_READY = "UiMatchStartPlayersNotReady";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { CODE_CREATE_INVALID_ARGS, KEY_CREATE_INVALID_ARGS },

                { CODE_JOIN_INVALID_ARGS_A, KEY_JOIN_INVALID_ARGS },
                { CODE_JOIN_MATCH_NOT_JOINABLE, KEY_JOIN_MATCH_NOT_JOINABLE },
                { CODE_JOIN_PLAYER_ALREADY_IN_MATCH, KEY_JOIN_ALREADY_IN_MATCH },
                { CODE_JOIN_GUEST_SLOT_TAKEN, KEY_JOIN_GUEST_SLOT_TAKEN },
                { CODE_JOIN_IN_OTHER_ACTIVE_MATCH, KEY_JOIN_IN_OTHER_ACTIVE_MATCH },
                { CODE_JOIN_OPERATION_CONFLICT, KEY_JOIN_OPERATION_CONFLICT },

                { CODE_SHARED_INVALID_ARGS, KEY_SHARED_INVALID_ARGS },
                { CODE_SHARED_MATCH_NOT_FOUND, KEY_SHARED_MATCH_NOT_FOUND },
                { CODE_SHARED_MATCH_NOT_IN_LOBBY, KEY_SHARED_MATCH_NOT_IN_LOBBY },
                { CODE_SHARED_HOST_NOT_AUTHORIZED, KEY_SHARED_HOST_NOT_AUTHORIZED },
                { CODE_SHARED_CONCURRENT_UPDATE, KEY_SHARED_CONCURRENT_UPDATE },
                { CODE_SHARED_UNEXPECTED_ERROR, KEY_SHARED_UNEXPECTED_ERROR },

                { CODE_VIS_ALREADY_DESIRED, KEY_VIS_ALREADY_DESIRED },
                { CODE_VIS_ALREADY_PRIVATE, KEY_VIS_ALREADY_DESIRED },
                { CODE_VIS_ALREADY_PUBLIC, KEY_VIS_ALREADY_DESIRED },

                { CODE_START_NOT_ENOUGH_PLAYERS, KEY_START_NOT_ENOUGH_PLAYERS },
                { CODE_START_PLAYERS_NOT_READY, KEY_START_PLAYERS_NOT_READY },
            };

        public UiKeyMapping Map(string faultCode)
        {
            string safe = (faultCode ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safe))
            {
                return UiKeyMapping.Unmapped();
            }

            if (IsNumeric(safe))
            {
                return UiKeyMapping.Unmapped();
            }

            if (!MapTable.TryGetValue(safe, out string uiKey))
            {
                return UiKeyMapping.Unmapped();
            }

            return UiKeyMapping.Mapped(uiKey);
        }

        private static bool IsNumeric(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }

            return value.Length > 0;
        }
    }
}
