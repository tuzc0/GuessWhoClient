using System;
using System.Collections.Generic;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public interface IValidationIssueMapper
    {
        ValidationIssueMapping Map(string validationKey);
    }

    public readonly record struct ValidationIssueMapping(
        bool IsMapped,
        string PropertyName,
        string MessageKey)
    {
        private const string EMPTY = "";

        public static ValidationIssueMapping Unmapped()
        {
            return new ValidationIssueMapping(false, EMPTY, EMPTY);
        }

        public static ValidationIssueMapping Mapped(string propertyName, string messageKey)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(messageKey))
            {
                return Unmapped();
            }

            return new ValidationIssueMapping(true, propertyName, messageKey);
        }
    }

    public sealed class CreateAccountValidationIssueMapper : IValidationIssueMapper
    {
        private static readonly IReadOnlyDictionary<string, ValidationIssueMapping> MapTable =
            new Dictionary<string, ValidationIssueMapping>(StringComparer.Ordinal)
            {
                { "Registration.Email.Required", ValidationIssueMapping.Mapped("Email", "UiValidationEmailRequired") },
                { "Registration.Email.TooLong", ValidationIssueMapping.Mapped("Email", "UiValidationEmailTooLong") },
                { "Registration.Email.InvalidFormat", ValidationIssueMapping.Mapped("Email", "UiValidationEmailFormat") },

                { "Registration.DisplayName.Required", ValidationIssueMapping.Mapped("DisplayName", "UiValidationDisplayNameRequired") },
                { "Registration.DisplayName.TooShort", ValidationIssueMapping.Mapped("DisplayName", "UiValidationDisplayNameTooShort") },
                { "Registration.DisplayName.TooLong", ValidationIssueMapping.Mapped("DisplayName", "UiValidationDisplayNameTooLong") },
                { "Registration.DisplayName.InvalidFormat", ValidationIssueMapping.Mapped("DisplayName", "UiValidationDisplayNameInvalidFormat") },

                { "Registration.Password.Required", ValidationIssueMapping.Mapped("Password", "UiValidationPasswordRequired") },
                { "Registration.Password.TooShort", ValidationIssueMapping.Mapped("Password", "UiValidationPasswordTooShort") },
                { "Registration.Password.TooLong", ValidationIssueMapping.Mapped("Password", "UiValidationPasswordTooLong") },

                { "Registration.ConfirmPassword.Required", ValidationIssueMapping.Mapped("ConfirmPassword", "UiValidationConfirmPasswordRequired") },
                { "Registration.ConfirmPassword.Mismatch", ValidationIssueMapping.Mapped("ConfirmPassword", "UiValidationPasswordDontMatch") }
            };

        public ValidationIssueMapping Map(string validationKey)
        {
            if (string.IsNullOrWhiteSpace(validationKey))
            {
                return ValidationIssueMapping.Unmapped();
            }

            return MapTable.TryGetValue(validationKey, out ValidationIssueMapping mapping)
                ? mapping
                : ValidationIssueMapping.Unmapped();
        }
    }
}
