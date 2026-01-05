namespace GuessWhoClient.Presentation.Navegation
{
    public sealed class AccountFlowContext
    {
        private const string EMPTY = "";

        public long PendingVerificationAccountId { get; private set; }
        public string PendingVerificationEmail { get; private set; } = EMPTY;

        public void SetPendingEmailVerification(long accountId, string email)
        {
            PendingVerificationAccountId = accountId;
            PendingVerificationEmail = email ?? EMPTY;
        }

        public void ClearPendingEmailVerification()
        {
            PendingVerificationAccountId = 0;
            PendingVerificationEmail = EMPTY;
        }
    }
}
