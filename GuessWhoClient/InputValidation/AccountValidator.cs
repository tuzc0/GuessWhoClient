using GuessWhoClient.Dtos;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GuessWhoClient.InputValidation
{
    public static class AccountValidator
    {
        private const int EMAIL_MAX_LENGTH = 254;
        private const int DISPLAY_NAME_MAX_LENGTH = 50;
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 64;

        private const int REGEX_VALIDATION_TIMEOUT_MS = 250;

        private const string ERROR_EMAIL_REQUIRED =
            "Email is required.";
        private const string ERROR_EMAIL_TOO_LONG =
            "Email cannot be longer than 254 characters.";
        private const string ERROR_EMAIL_INVALID_FORMAT =
            "Email format is not valid. Example: user@example.com.";

        private const string ERROR_DISPLAY_NAME_REQUIRED =
            "Display name is required.";
        private const string ERROR_DISPLAY_NAME_TOO_LONG =
            "Display name cannot be longer than 50 characters.";
        private const string ERROR_DISPLAY_NAME_INVALID_CHARS =
            "Display name can only contain letters, numbers and spaces.";

        private const string ERROR_PASSWORD_REQUIRED =
            "Password is required.";
        private const string ERROR_PASSWORD_LENGTH =
            "Password must be between 8 and 64 characters long.";
        private const string ERROR_PASSWORD_MISSING_LETTER =
            "Password must contain at least one letter (A–Z or a–z).";
        private const string ERROR_PASSWORD_MISSING_DIGIT =
            "Password must contain at least one number (0–9).";
        private const string ERROR_PASSWORD_MISSING_SPECIAL =
            "Password must contain at least one special character (for example: !, ?, #, @).";
        private const string ERROR_PASSWORD_CONFIRM_MISMATCH =
            "Password and confirmation password do not match.";

        private static readonly Regex EmailRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(REGEX_VALIDATION_TIMEOUT_MS));

        private static readonly Regex DisplayNameRegex =
            new Regex(@"^[A-Za-z0-9 ]+$", RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(REGEX_VALIDATION_TIMEOUT_MS));

        public static List<string> ValidateLoginForm(LoginInput login)
        {
            var errors = new List<string>();

            ValidateEmail(login.Email, errors);
            ValidatePassword(login.Password, null, errors);
            
            return errors;
        }

        public static List<string> ValidateForm(AccountProfileInput accountProfile)
        {
            var errors = new List<string>();

            ValidateEmail(accountProfile.Email, errors);
            ValidateDisplayName(accountProfile.DisplayName, errors);
            ValidatePassword(accountProfile.Password, accountProfile.ConfirmPassword, errors);

            return errors;
        }

        private static void ValidateEmail(string email, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(ERROR_EMAIL_REQUIRED);
                return;
            }

            if (email.Length > EMAIL_MAX_LENGTH)
            {
                errors.Add(ERROR_EMAIL_TOO_LONG);
            }

            if (!EmailRegex.IsMatch(email))
            {
                errors.Add(ERROR_EMAIL_INVALID_FORMAT);
            }
        }

        private static void ValidateDisplayName(string displayName, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add(ERROR_DISPLAY_NAME_REQUIRED);
                return;
            }

            if (displayName.Length > DISPLAY_NAME_MAX_LENGTH)
            {
                errors.Add(ERROR_DISPLAY_NAME_TOO_LONG);
            }

            if (!DisplayNameRegex.IsMatch(displayName))
            {
                errors.Add(ERROR_DISPLAY_NAME_INVALID_CHARS);
            }
        }

        private static void ValidatePassword(string password, string confirmPassword, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add(ERROR_PASSWORD_REQUIRED);
                return;
            }

            if (password.Length < PASSWORD_MIN_LENGTH || password.Length > PASSWORD_MAX_LENGTH)
            {
                errors.Add(ERROR_PASSWORD_LENGTH);
            }

            var hasLetter = false;
            var hasDigit = false;
            var hasSpecial = false;

            foreach (var character in password)
            {
                if (char.IsLetter(character))
                {
                    hasLetter = true;
                }
                else if (char.IsDigit(character))
                {
                    hasDigit = true;
                }
                else
                {
                    hasSpecial = true;
                }
            }

            if (!hasLetter)
            {
                errors.Add(ERROR_PASSWORD_MISSING_LETTER);
            }

            if (!hasDigit)
            {
                errors.Add(ERROR_PASSWORD_MISSING_DIGIT);
            }

            if (!hasSpecial)
            {
                errors.Add(ERROR_PASSWORD_MISSING_SPECIAL);
            }

            if (!string.IsNullOrEmpty(confirmPassword) &&
                !string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                errors.Add(ERROR_PASSWORD_CONFIRM_MISMATCH);
            }
        }
    }
}
