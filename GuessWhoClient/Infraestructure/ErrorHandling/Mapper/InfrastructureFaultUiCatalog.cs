using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class InfrastructureFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { InfrastructureFaultKeys.CODE_REQUEST_NULL, "UiInfra.InvalidRequest" },
                { InfrastructureFaultKeys.CODE_UNEXPECTED_ERROR, "UiInfra.UnexpectedError" },

                { InfrastructureFaultKeys.CODE_DATABASE_COMMAND_TIMEOUT, "UiInfra.Database.Timeout" },
                { InfrastructureFaultKeys.CODE_DATABASE_CONNECTION_FAILURE, "UiInfra.Database.Connection" },
                { InfrastructureFaultKeys.CODE_DATABASE_NETWORK_PATH_NOT_FOUND, "UiInfra.Database.Connection" },
                { InfrastructureFaultKeys.CODE_DATABASE_SERVER_NOT_FOUND, "UiInfra.Database.Connection" },
                { InfrastructureFaultKeys.CODE_DATABASE_TRANSPORT_LEVEL_ERROR, "UiInfra.Database.Connection" },

                { InfrastructureFaultKeys.CODE_DATABASE_NOT_FOUND, "UiInfra.Database.NotFound" },
                { InfrastructureFaultKeys.CODE_DATABASE_UNREACHABLE, "UiInfra.Database.Unreachable" },

                { InfrastructureFaultKeys.CODE_DEFAULT_AVATAR_NOT_CONFIGURED, "UiInfra.DefaultAvatarNotConfigured" },
            };

        public string ResolveUiKey(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return EMPTY;
            }

            return MapTable.TryGetValue(faultCode, out string key)
                ? key ?? EMPTY
                : EMPTY;
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
