namespace GuessWhoClient.Dtos
{
    public class AccountProfileInput
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public AccountProfileInput(string email, string displayName, string password, string confirmPassword)
        {
            Email = (email ?? string.Empty).Trim().ToLowerInvariant();
            DisplayName = (displayName ?? string.Empty).Trim();
            Password = password ?? string.Empty;
            ConfirmPassword = confirmPassword ?? string.Empty;
        }
    }
}
