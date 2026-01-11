using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class LeaderboardFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_REQUEST_NULL = "UiLeaderboardRequestNull";
        private const string KEY_INVALID_TOP_N = "UiLeaderboardInvalidTopN";
        private const string KEY_USER_NOT_FOUND = "UiLeaderboardUserNotFound";
        private const string KEY_TECHNICAL_ERROR = "UiLeaderboardTechnicalError";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { LeaderboardFaultKeys.CODE_REQUEST_NULL, KEY_REQUEST_NULL },
                { LeaderboardFaultKeys.CODE_INVALID_TOP_N, KEY_INVALID_TOP_N },
                { LeaderboardFaultKeys.CODE_USER_NOT_FOUND, KEY_USER_NOT_FOUND },
                { LeaderboardFaultKeys.CODE_UNEXPECTED_ERROR, KEY_TECHNICAL_ERROR },
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