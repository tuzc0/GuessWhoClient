using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class UserRegistrationFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { UserRegistrationFaultKeys.CODE_REQUEST_NULL, "UiRegister.InvalidRequest" },
                { UserRegistrationFaultKeys.CODE_ARGS_REQUIRED, "UiRegister.InvalidRequest" },
                { UserRegistrationFaultKeys.CODE_VALIDATION_FAILED, "UiRegister.InvalidRequest" },

                { UserRegistrationFaultKeys.CODE_EMAIL_REQUIRED, "UiRegister.Email.Required" },
                { UserRegistrationFaultKeys.CODE_EMAIL_INVALID, "UiRegister.Email.Invalid" },
                { UserRegistrationFaultKeys.CODE_EMAIL_ALREADY_EXISTS, "UiRegister.Email.AlreadyExists" },

                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_REQUIRED, "UiRegister.DisplayName.Required" },
                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_INVALID, "UiRegister.DisplayName.Invalid" },

                { UserRegistrationFaultKeys.CODE_PASSWORD_REQUIRED, "UiRegister.Password.Required" },
                { UserRegistrationFaultKeys.CODE_PASSWORD_INVALID, "UiRegister.Password.Invalid" },

                { UserRegistrationFaultKeys.CODE_TOKEN_CREATION_FAILED, "UiRegister.TechnicalError" },
                { UserRegistrationFaultKeys.CODE_NOWUTC_REQUIRED, "UiRegister.TechnicalError" },
                { UserRegistrationFaultKeys.CODE_UNEXPECTED_ERROR, "UiRegister.TechnicalError" },
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
