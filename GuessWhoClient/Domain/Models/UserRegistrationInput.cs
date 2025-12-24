namespace GuessWhoClient.Domain.Models
{
    public class UserRegistrationInput
    {
        public string Username { get; }
        public string Password { get; }
        public string Email { get; }

        public UserRegistrationInput(string username, string password, string email)
        {
            Username = username;
            Password = password;
            Email = email;
        }
    }
}
