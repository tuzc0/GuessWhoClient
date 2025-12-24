namespace GuessWhoClient.Application.Results
{
    public class RegisterUserResult
    {
        public RegisterUserResult(bool emailVerificationRequired, long accountId, string email)
        {
            EmailVerificationRequired = emailVerificationRequired;
            AccountId = accountId;
            Email = email;
        }

        public bool EmailVerificationRequired { get; }
        public long AccountId { get; }
        public string Email { get; }
    }
}
