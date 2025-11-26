namespace GuessWhoClient.Dtos
{
    public sealed class PasswordChangeRequest
    {
        public string CurrentPassword { get; }
        public string NewPassword { get; }
        public string ConfirmPassword { get; }

        public PasswordChangeRequest(string currentPassword, string newPassword, string confirmPassword)
        {
            CurrentPassword = currentPassword ?? string.Empty;
            NewPassword = newPassword ?? string.Empty;
            ConfirmPassword = confirmPassword ?? string.Empty;
        }
    }
}
