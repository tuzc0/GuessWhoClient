namespace GuessWhoClient.Dtos
{
    public sealed class LoginInput
    {
        public string Email { get; }
        public string Password { get; }

        public LoginInput(string email, string password)
        {
            Email = (email ?? string.Empty).Trim().ToLowerInvariant();
            Password = password ?? string.Empty;
        }
    }
}
