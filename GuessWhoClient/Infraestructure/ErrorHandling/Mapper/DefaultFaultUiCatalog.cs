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

        private static readonly IReadOnlyDictionary<string, string> MapTable =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { LoginFaultKeys.CODE_INVALID_CREDENTIALS, "LoginInvalidCredentials" },
                { LoginFaultKeys.CODE_ACCOUNT_LOCKED, "LoginAccountLocked" },
                { LoginFaultKeys.CODE_PROFILE_ALREADY_ACTIVE, "LoginProfileAlreadyActive" },
                { LoginFaultKeys.CODE_UNEXPECTED_ERROR, "UiGenericError" },
                { LoginCoordinatorFaultKeys.CODE_PROFILE_MARK_ACTIVE_FAILED, "LoginProfileActivationFailed" },
                { WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, "UiEndpointNotFound" },
                { WcfTechnicalFaultCodes.TIMEOUT, "UiTimeout" },
                { WcfTechnicalFaultCodes.COMMUNICATION_ERROR, "UiCommunicationError" },
                { WcfTechnicalFaultCodes.SECURITY_ERROR, "UiSecurityError" },
                { WcfTechnicalFaultCodes.NULL_RESPONSE, "UiGenericError" },
                { WcfTechnicalFaultCodes.UNEXPECTED, "UiGenericError" },
                { UserValidationCodes.INVALID_REQUEST, "UiInvalidRequest" },
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

            if (MapTable.TryGetValue(faultCode, out var key))
            {
                return key ?? EMPTY;
            }

            return EMPTY;
        }
    }
}
