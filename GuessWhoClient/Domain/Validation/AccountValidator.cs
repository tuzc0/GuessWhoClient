using System;
using System.Collections.Generic;

namespace GuessWhoClient.InputValidation
{
    public static class AccountValidator
    {
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 64;

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

        public static List<string> ValidatePasswordChange(string newPassword, string confirmPassword)
        {
            var errors = new List<string>();
            ValidatePassword(newPassword, confirmPassword, errors);
            return errors;
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
