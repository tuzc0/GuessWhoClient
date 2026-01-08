namespace GuessWhoClient.Domain.Validation
{
    public readonly struct ValidationIssueMapping 
    {
        private const string EMPTY = "";

        private ValidationIssueMapping(bool isMapped, string propertyName, string messageKey)
        {
            IsMapped = isMapped;
            PropertyName = propertyName ?? EMPTY;
            MessageKey = messageKey ?? EMPTY;
        }

        public bool IsMapped { get; }
        public string PropertyName { get; }
        public string MessageKey { get; }

        public static ValidationIssueMapping Unmapped()
        {
            return new ValidationIssueMapping(false, EMPTY, EMPTY);
        }

        public static ValidationIssueMapping Mapped(string propertyName, string messageKey)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(messageKey))
            {
                return Unmapped();
            }

            return new ValidationIssueMapping(true, propertyName, messageKey);
        }
    }
}
