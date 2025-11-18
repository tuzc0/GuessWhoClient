using System;

namespace GuessWhoClient.Session
{
    public sealed class SessionContext
    {
        private const int DEFAULT_USER_ID = 0;
        private const string EMPTY = "";

        private static readonly Lazy<SessionContext> LazyInstance =
            new Lazy<SessionContext>(() => new SessionContext());

        public static SessionContext Current => LazyInstance.Value;

        private SessionContext()
        {
            Reset();
        }

        public long UserId { get; private set; }
        public string DisplayName { get; private set; }
        public string Email { get; private set; }
        public bool AuthToken { get; private set; }

        public bool IsAuthenticated =>
            UserId > DEFAULT_USER_ID &&
            !string.IsNullOrWhiteSpace(Email);

        public void SignIn(long userId, string displayName, string email, bool authToken)
        {
            if (userId <= DEFAULT_USER_ID)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            UserId = userId;
            DisplayName = displayName ?? EMPTY;
            Email = email ?? EMPTY;
            AuthToken = authToken;
        }

        public void UpdateDisplayName(string newDisplayName)
        {
            DisplayName = newDisplayName ?? EMPTY;
        }

        public void UpdateEmail(string newEmail)
        {
            Email = newEmail ?? EMPTY;
        }

        public void UpdateAuthToken(bool newToken)
        {
            AuthToken = newToken;
        }

        public void SignOut()
        {
            Reset();
        }

        private void Reset()
        {
            UserId = DEFAULT_USER_ID;
            DisplayName = EMPTY;
            Email = EMPTY;
            AuthToken = false;
        }
    }
}
