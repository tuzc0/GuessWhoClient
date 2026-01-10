using GuessWhoCore.Validation;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Domain.Validation
{
    public sealed class UserValidationIssueMapper : IValidationIssueMapper
    {
        private const string PROP_EMAIL = "Email";
        private const string PROP_DISPLAY_NAME = "DisplayName";
        private const string PROP_PASSWORD = "Password";
        private const string PROP_CONFIRM_PASSWORD = "ConfirmPassword";
        private const string PROP_CURRENT_PASSWORD = "CurrentPassword";
        private const string PROP_AVATAR_ID = "AvatarId";

        private static readonly IReadOnlyDictionary<string, ValidationIssueMapping> MapTable =
            new Dictionary<string, ValidationIssueMapping>(StringComparer.Ordinal)
            {
            { UserValidationCodes.EMAIL_REQUIRED, ValidationIssueMapping.Mapped(PROP_EMAIL, "UiValidationEmailRequired") },
            { UserValidationCodes.EMAIL_TOO_LONG, ValidationIssueMapping.Mapped(PROP_EMAIL, "UiValidationEmailTooLong") },
            { UserValidationCodes.EMAIL_INVALID_FORMAT, ValidationIssueMapping.Mapped(PROP_EMAIL, "UiValidationEmailFormat") },

            { UserValidationCodes.DISPLAY_NAME_REQUIRED, ValidationIssueMapping.Mapped(PROP_DISPLAY_NAME, "UiValidationDisplayNameRequired") },
            { UserValidationCodes.DISPLAY_NAME_TOO_SHORT, ValidationIssueMapping.Mapped(PROP_DISPLAY_NAME, "UiValidationDisplayNameTooShort") },
            { UserValidationCodes.DISPLAY_NAME_TOO_LONG, ValidationIssueMapping.Mapped(PROP_DISPLAY_NAME, "UiValidationDisplayNameTooLong") },
            { UserValidationCodes.DISPLAY_NAME_INVALID_FORMAT, ValidationIssueMapping.Mapped(PROP_DISPLAY_NAME, "UiValidationDisplayNameInvalidFormat") },

            { UserValidationCodes.PASSWORD_REQUIRED, ValidationIssueMapping.Mapped(PROP_PASSWORD, "UiValidationPasswordRequired") },
            { UserValidationCodes.PASSWORD_TOO_SHORT, ValidationIssueMapping.Mapped(PROP_PASSWORD, "UiValidationPasswordTooShort") },
            { UserValidationCodes.PASSWORD_TOO_LONG, ValidationIssueMapping.Mapped(PROP_PASSWORD, "UiValidationPasswordTooLong") },
            { UserValidationCodes.PASSWORD_INVALID_FORMAT, ValidationIssueMapping.Mapped(PROP_PASSWORD, "UiValidationPasswordInvalid") },

            { UserValidationCodes.CONFIRM_PASSWORD_REQUIRED, ValidationIssueMapping.Mapped(PROP_CONFIRM_PASSWORD, "UiValidationConfirmPasswordRequired") },
            { UserValidationCodes.CONFIRM_PASSWORD_MISMATCH, ValidationIssueMapping.Mapped(PROP_CONFIRM_PASSWORD, "UiValidationPasswordDontMatch") },

            { UserValidationCodes.CURRENT_PASSWORD_REQUIRED, ValidationIssueMapping.Mapped(PROP_CURRENT_PASSWORD, "UiValidationCurrentPasswordRequired") },
            { UserValidationCodes.AVATAR_ID_TOO_LONG, ValidationIssueMapping.Mapped(PROP_AVATAR_ID, "UiValidationAvatarIdTooLong") },
            };

        public ValidationIssueMapping Map(string issueKey)
        {
            if (string.IsNullOrWhiteSpace(issueKey))
            {
                return ValidationIssueMapping.Unmapped();
            }

            return MapTable.TryGetValue(issueKey, out ValidationIssueMapping mapping)
                ? mapping
                : ValidationIssueMapping.Unmapped();
        }
    }
}
