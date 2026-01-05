using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public sealed class CreateAccountUiFaultMapper : IUiFaultMapper
    {
        private const string KEY_UI_EMAIL_ALREADY_EXISTS = "FaultDuplicateEmail";
        private const string KEY_UI_REQUEST_NULL = "USER_REQUEST_NULL";

        private const string KEY_UI_EMAIL_REQUIRED = "UiValidationEmailRequired";
        private const string KEY_UI_EMAIL_INVALID = "UiValidationEmailFormat";

        private const string KEY_UI_DISPLAYNAME_REQUIRED = "UiValidationDisplayNameRequired";
        private const string KEY_UI_DISPLAYNAME_INVALID = "UiValidationDisplayNameInvalid";

        private const string KEY_UI_PASSWORD_REQUIRED = "UiValidationPasswordRequired";
        private const string KEY_UI_PASSWORD_INVALID = "UiValidationPasswordInvalid";

        private const string KEY_UI_GENERIC_ERROR = "FaultUnexpected";

        public UiKeyMapping Map(string faultCode)
        {
            if (string.IsNullOrWhiteSpace(faultCode))
            {
                return UiKeyMapping.Unmapped();
            }

            if (string.Equals(faultCode, InfrastructureFaultKeys.CODE_REQUEST_NULL, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_REQUEST_NULL);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_EMAIL_ALREADY_EXISTS, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_EMAIL_ALREADY_EXISTS);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_EMAIL_REQUIRED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_EMAIL_REQUIRED);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_EMAIL_INVALID, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_EMAIL_INVALID);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_DISPLAYNAME_REQUIRED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_DISPLAYNAME_REQUIRED);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_DISPLAYNAME_INVALID, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_DISPLAYNAME_INVALID);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_PASSWORD_REQUIRED, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_PASSWORD_REQUIRED);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_PASSWORD_INVALID, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_PASSWORD_INVALID);
            }

            if (string.Equals(faultCode, UserRegistrationFaultKeys.CODE_UNEXPECTED_ERROR, StringComparison.Ordinal))
            {
                return UiKeyMapping.Mapped(KEY_UI_GENERIC_ERROR);
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
