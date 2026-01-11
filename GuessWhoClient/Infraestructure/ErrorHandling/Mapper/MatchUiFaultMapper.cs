using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class MatchUiFaultMapper : IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string CODE_CREATE_INVALID_ARGS = "INVALID_ARGS";

        private const string CODE_JOIN_INVALID_ARGS_A = "MATCH_INVALID_ARGS";
        private const string CODE_JOIN_INVALID_ARGS_B = "InvalidArgs";

        private const string CODE_JOIN_MATCH_NOT_FOUND = "MatchNotFound";
        private const string CODE_JOIN_MATCH_NOT_JOINABLE = "MatchNotJoinable";
        private const string CODE_JOIN_PLAYER_ALREADY_IN_MATCH = "PlayerAlreadyInMatch";
        private const string CODE_JOIN_GUEST_SLOT_TAKEN = "GuestSlotTaken";
        private const string CODE_JOIN_IN_OTHER_ACTIVE_MATCH = "InOtherActiveMatch";
        private const string CODE_JOIN_OPERATION_CONFLICT = "OperationConflict";

        private const string KEY_CREATE_INVALID_ARGS = "UiMatchCreateInvalidArgs";
        private const string KEY_JOIN_INVALID_ARGS = "UiMatchJoinInvalidArgs";

        private const string KEY_JOIN_MATCH_NOT_FOUND = "UiMatchJoinMatchNotFound";
        private const string KEY_JOIN_MATCH_NOT_JOINABLE = "UiMatchJoinMatchNotJoinable";
        private const string KEY_JOIN_ALREADY_IN_MATCH = "UiMatchJoinPlayerAlreadyInMatch";
        private const string KEY_JOIN_GUEST_SLOT_TAKEN = "UiMatchJoinGuestSlotTaken";
        private const string KEY_JOIN_IN_OTHER_ACTIVE_MATCH = "UiMatchJoinInOtherActiveMatch";
        private const string KEY_JOIN_OPERATION_CONFLICT = "UiMatchJoinOperationConflict";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { CODE_CREATE_INVALID_ARGS, KEY_CREATE_INVALID_ARGS },

                { CODE_JOIN_INVALID_ARGS_A, KEY_JOIN_INVALID_ARGS },
                { CODE_JOIN_INVALID_ARGS_B, KEY_JOIN_INVALID_ARGS },

                { CODE_JOIN_MATCH_NOT_FOUND, KEY_JOIN_MATCH_NOT_FOUND },
                { CODE_JOIN_MATCH_NOT_JOINABLE, KEY_JOIN_MATCH_NOT_JOINABLE },
                { CODE_JOIN_PLAYER_ALREADY_IN_MATCH, KEY_JOIN_ALREADY_IN_MATCH },
                { CODE_JOIN_GUEST_SLOT_TAKEN, KEY_JOIN_GUEST_SLOT_TAKEN },
                { CODE_JOIN_IN_OTHER_ACTIVE_MATCH, KEY_JOIN_IN_OTHER_ACTIVE_MATCH },
                { CODE_JOIN_OPERATION_CONFLICT, KEY_JOIN_OPERATION_CONFLICT },
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
