using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Validation;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public sealed class DefaultFaultUiCatalog : IFaultUiCatalog
    {
        private const string EMPTY = "";

        private const string KEY_GENERIC_ERROR = "UiGenericError";
        private const string KEY_INVALID_REQUEST = "UiInvalidRequest";

        private const string KEY_LOGIN_INVALID_CREDENTIALS = "UiLoginInvalidCredentials";
        private const string KEY_LOGIN_ACCOUNT_LOCKED = "UiLoginAccountLocked";
        private const string KEY_LOGIN_PROFILE_ALREADY_ACTIVE = "UiLoginProfileAlreadyActive";
        private const string KEY_LOGIN_PROFILE_ACTIVATION_FAILED = "UiLoginProfileActivationFailed";
        private const string KEY_LOGOUT_FAILED = "UiLogoutFailed";
        private const string KEY_SESSION_INVALID_OR_EXPIRED = "UiSessionInvalidOrExpired";
        private const string KEY_SESSION_USER_ID_INVALID = "UiSessionInvalidOrExpired";

        private const string KEY_WCF_ENDPOINT_NOT_FOUND = "UiCommunicationEndpointNotFound";
        private const string KEY_WCF_TIMEOUT = "UiCommunicationTimeout";
        private const string KEY_WCF_COMMUNICATION = "UiCommunicationError";
        private const string KEY_WCF_SECURITY = "UiCommunicationSecurityError";

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { LoginFaultKeys.CODE_REQUEST_NULL, KEY_INVALID_REQUEST },
                { LoginFaultKeys.CODE_INVALID_CREDENTIALS, KEY_LOGIN_INVALID_CREDENTIALS },
                { LoginFaultKeys.CODE_ACCOUNT_LOCKED, KEY_LOGIN_ACCOUNT_LOCKED },
                { LoginFaultKeys.CODE_PROFILE_ALREADY_ACTIVE, KEY_LOGIN_PROFILE_ALREADY_ACTIVE },
                { LoginFaultKeys.CODE_LOGOUT_FAILED, KEY_LOGOUT_FAILED },
                { LoginFaultKeys.CODE_UNEXPECTED_ERROR, KEY_GENERIC_ERROR },
                { GameSessionFaultKeys.CODE_USER_ID_INVALID, KEY_SESSION_USER_ID_INVALID},

                { LoginCoordinatorFaultKeys.CODE_USER_ID_INVALID, KEY_SESSION_INVALID_OR_EXPIRED },
                { LoginCoordinatorFaultKeys.CODE_PROFILE_MARK_ACTIVE_FAILED, KEY_LOGIN_PROFILE_ACTIVATION_FAILED },
                { LoginCoordinatorFaultKeys.CODE_LOGOUT_TERMINATE_SESSIONS_FAILED, KEY_LOGOUT_FAILED },
                { LoginCoordinatorFaultKeys.CODE_LOGOUT_MARK_INACTIVE_FAILED, KEY_LOGOUT_FAILED },

                { WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, KEY_WCF_ENDPOINT_NOT_FOUND },
                { WcfTechnicalFaultCodes.TIMEOUT, KEY_WCF_TIMEOUT },
                { WcfTechnicalFaultCodes.COMMUNICATION_ERROR, KEY_WCF_COMMUNICATION },
                { WcfTechnicalFaultCodes.SECURITY_ERROR, KEY_WCF_SECURITY },
                { WcfTechnicalFaultCodes.NULL_RESPONSE, KEY_GENERIC_ERROR },
                { WcfTechnicalFaultCodes.UNEXPECTED, KEY_GENERIC_ERROR },

                { UserValidationCodes.INVALID_REQUEST, KEY_INVALID_REQUEST },
                { UserValidationCodes.EMAIL_REQUIRED, "UiValidationEmailRequired" },
                { UserValidationCodes.EMAIL_TOO_LONG, "UiValidationEmailTooLong" },
                { UserValidationCodes.EMAIL_INVALID_FORMAT, "UiValidationEmailFormat" },
                { UserValidationCodes.PASSWORD_REQUIRED, "UiValidationPasswordRequired" },
                { UserValidationCodes.PASSWORD_TOO_SHORT, "UiValidationPasswordTooShort" },
                { UserValidationCodes.PASSWORD_TOO_LONG, "UiValidationPasswordTooLong" },
                { UserValidationCodes.DISPLAY_NAME_REQUIRED, "UiValidationDisplayNameRequired" },
                { UserValidationCodes.DISPLAY_NAME_TOO_SHORT, "UiValidationDisplayNameTooShort" },
                { UserValidationCodes.DISPLAY_NAME_TOO_LONG, "UiValidationDisplayNameTooLong" },
                { UserValidationCodes.DISPLAY_NAME_INVALID_FORMAT, "UiValidationDisplayNameInvalidFormat" },
                { UserValidationCodes.CONFIRM_PASSWORD_REQUIRED, "UiValidationConfirmPasswordRequired" },
                { UserValidationCodes.CONFIRM_PASSWORD_MISMATCH, "UiValidationPasswordDontMatch" },
                { UserValidationCodes.AVATAR_ID_TOO_LONG, "UiValidationAvatarIdTooLong" },
                { UserValidationCodes.CURRENT_PASSWORD_REQUIRED, "UiValidationCurrentPasswordRequired" },
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
    }
}
