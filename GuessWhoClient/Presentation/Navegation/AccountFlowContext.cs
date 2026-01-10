namespace GuessWhoClient.Presentation.Navegation
{
    public sealed class AccountFlowContext
    {
        private const string EMPTY = "";

        public long PendingVerificationAccountId { get; private set; }
        public string PendingVerificationEmail { get; private set; } = EMPTY;

        public GameScreenType PendingVerificationReturnScreen { get; private set; } = GameScreenType.Login;

        public void SetPendingEmailVerification(long accountId, string email, GameScreenType returnScreen)
        {
            PendingVerificationAccountId = accountId;
            PendingVerificationEmail = email ?? EMPTY;
            PendingVerificationReturnScreen = returnScreen;
        }

        public void ClearPendingEmailVerification()
        {
            PendingVerificationAccountId = 0;
            PendingVerificationEmail = EMPTY;
            PendingVerificationReturnScreen = GameScreenType.Login;
        }
    }
}
