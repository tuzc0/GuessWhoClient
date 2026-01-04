namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public readonly record struct UiKeyMapping(bool IsMapped, string UiKey)
    {
        private const string EMPTY = "";

        public static UiKeyMapping Unmapped()
        {
            return new UiKeyMapping(false, EMPTY);
        }

        public static UiKeyMapping Mapped(string uiKey)
        {
            if(string.IsNullOrWhiteSpace(uiKey))
            {
                return Unmapped();
            }

            return new UiKeyMapping(true, uiKey);
        }
    }
}
