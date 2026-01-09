using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class InfrastructureFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_REQUEST_NULL = "UiInfrastructureRequestNull";
        private const string KEY_UNEXPECTED = "UiInfrastructureUnexpectedError";

        private const string KEY_DB_TIMEOUT = "UiInfrastructureDatabaseCommandTimeout";
        private const string KEY_DB_CONNECTION_FAILURE = "UiInfrastructureDatabaseConnectionFailure";
        private const string KEY_DB_NETWORK_PATH_NOT_FOUND = "UiInfrastructureDatabaseNetworkPathNotFound";
        private const string KEY_DB_SERVER_NOT_FOUND = "UiInfrastructureDatabaseServerNotFound";
        private const string KEY_DB_TRANSPORT_LEVEL_ERROR = "UiInfrastructureDatabaseTransportLevelError";
        private const string KEY_DB_NOT_FOUND = "UiInfrastructureDatabaseNotFound";
        private const string KEY_DB_UNREACHABLE = "UiInfrastructureDatabaseUnreachable";

        private const string KEY_DEFAULT_AVATAR_NOT_CONFIGURED = "UiInfrastructureDefaultAvatarNotConfigured";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { InfrastructureFaultKeys.CODE_REQUEST_NULL, KEY_REQUEST_NULL },
                { InfrastructureFaultKeys.CODE_UNEXPECTED_ERROR, KEY_UNEXPECTED },

                { InfrastructureFaultKeys.CODE_DATABASE_COMMAND_TIMEOUT, KEY_DB_TIMEOUT },
                { InfrastructureFaultKeys.CODE_DATABASE_CONNECTION_FAILURE, KEY_DB_CONNECTION_FAILURE },
                { InfrastructureFaultKeys.CODE_DATABASE_NETWORK_PATH_NOT_FOUND, KEY_DB_NETWORK_PATH_NOT_FOUND },
                { InfrastructureFaultKeys.CODE_DATABASE_SERVER_NOT_FOUND, KEY_DB_SERVER_NOT_FOUND },
                { InfrastructureFaultKeys.CODE_DATABASE_TRANSPORT_LEVEL_ERROR, KEY_DB_TRANSPORT_LEVEL_ERROR },

                { InfrastructureFaultKeys.CODE_DATABASE_NOT_FOUND, KEY_DB_NOT_FOUND },
                { InfrastructureFaultKeys.CODE_DATABASE_UNREACHABLE, KEY_DB_UNREACHABLE },

                { InfrastructureFaultKeys.CODE_DEFAULT_AVATAR_NOT_CONFIGURED, KEY_DEFAULT_AVATAR_NOT_CONFIGURED },
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
