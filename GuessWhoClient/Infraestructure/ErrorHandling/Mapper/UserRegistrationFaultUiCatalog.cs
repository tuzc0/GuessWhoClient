using GuessWhoCore.Contracts.Faults;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class UserRegistrationFaultUiCatalog : IFaultUiCatalog, IUiFaultMapper
    {
        private const string EMPTY = "";

        private const string KEY_INVALID_REQUEST = "UiRegisterInvalidRequest";
        private const string KEY_EMAIL_REQUIRED = "UiRegisterEmailRequired";
        private const string KEY_EMAIL_INVALID = "UiRegisterEmailInvalid";
        private const string KEY_EMAIL_ALREADY_EXISTS = "UiRegisterEmailAlreadyExists";
        private const string KEY_DISPLAY_NAME_REQUIRED = "UiRegisterDisplayNameRequired";
        private const string KEY_DISPLAY_NAME_INVALID = "UiRegisterDisplayNameInvalid";
        private const string KEY_PASSWORD_REQUIRED = "UiRegisterPasswordRequired";
        private const string KEY_PASSWORD_INVALID = "UiRegisterPasswordInvalid";
        private const string KEY_TECHNICAL_ERROR = "UiRegisterTechnicalError";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { UserRegistrationFaultKeys.CODE_REQUEST_NULL, KEY_INVALID_REQUEST },
                { UserRegistrationFaultKeys.CODE_ARGS_REQUIRED, KEY_INVALID_REQUEST },
                { UserRegistrationFaultKeys.CODE_VALIDATION_FAILED, KEY_INVALID_REQUEST },

                { UserRegistrationFaultKeys.CODE_EMAIL_REQUIRED, KEY_EMAIL_REQUIRED },
                { UserRegistrationFaultKeys.CODE_EMAIL_INVALID, KEY_EMAIL_INVALID },
                { UserRegistrationFaultKeys.CODE_EMAIL_ALREADY_EXISTS, KEY_EMAIL_ALREADY_EXISTS },

                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_REQUIRED, KEY_DISPLAY_NAME_REQUIRED },
                { UserRegistrationFaultKeys.CODE_DISPLAYNAME_INVALID, KEY_DISPLAY_NAME_INVALID },

                { UserRegistrationFaultKeys.CODE_PASSWORD_REQUIRED, KEY_PASSWORD_REQUIRED },
                { UserRegistrationFaultKeys.CODE_PASSWORD_INVALID, KEY_PASSWORD_INVALID },

                { UserRegistrationFaultKeys.CODE_TOKEN_CREATION_FAILED, KEY_TECHNICAL_ERROR },
                { UserRegistrationFaultKeys.CODE_NOWUTC_REQUIRED, KEY_TECHNICAL_ERROR },
                { UserRegistrationFaultKeys.CODE_UNEXPECTED_ERROR, KEY_TECHNICAL_ERROR },
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
