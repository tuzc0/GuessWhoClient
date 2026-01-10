using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    internal static class MatchBusinessErrorMapper
    {
        private const string EMPTY = "";

        private const string KEY_MATCH_JOIN_FAILED = "Match.JoinFailed";
        private const string KEY_MATCH_NOT_FOUND = "Match.NotFound";
        private const string KEY_MATCH_NOT_JOINABLE = "Match.NotJoinable";
        private const string KEY_MATCH_ALREADY_IN_MATCH = "Match.AlreadyInMatch";
        private const string KEY_MATCH_GUEST_SLOT_TAKEN = "Match.GuestSlotTaken";
        private const string KEY_MATCH_IN_OTHER_ACTIVE = "Match.InOtherActiveMatch";
        private const string KEY_MATCH_CONFLICT = "Match.OperationConflict";
        private const string KEY_MATCH_INVALID_ARGS = "Match.InvalidArgs";

        private const string KEY_MATCH_CREATE_FAILED = "Match.CreateFailed";

        private static readonly IReadOnlyDictionary<string, string> JoinCodeToUiKey =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "MatchNotFound", KEY_MATCH_NOT_FOUND },
                { "MatchNotJoinable", KEY_MATCH_NOT_JOINABLE },
                { "PlayerAlreadyInMatch", KEY_MATCH_ALREADY_IN_MATCH },
                { "GuestSlotTaken", KEY_MATCH_GUEST_SLOT_TAKEN },
                { "InOtherActiveMatch", KEY_MATCH_IN_OTHER_ACTIVE },
                { "OperationConflict", KEY_MATCH_CONFLICT },
                { "InvalidArgs", KEY_MATCH_INVALID_ARGS },
            };

        private static readonly IReadOnlyDictionary<string, string> CreateCodeToUiKey =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "INVALID_ARGS", KEY_MATCH_INVALID_ARGS },
            };

        internal static string MapJoinBusinessCodeToUiKey(string code)
        {
            string safe = (code ?? EMPTY).Trim();

            return JoinCodeToUiKey.TryGetValue(safe, out string key)
                ? key
                : KEY_MATCH_JOIN_FAILED;
        }

        internal static string MapCreateBusinessCodeToUiKey(string code)
        {
            string safe = (code ?? EMPTY).Trim();

            return CreateCodeToUiKey.TryGetValue(safe, out string key)
                ? key
                : KEY_MATCH_CREATE_FAILED;
        }
    }
}
