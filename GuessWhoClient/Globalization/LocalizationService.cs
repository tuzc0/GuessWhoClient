namespace GuessWhoClient.Globalization
{
    public sealed class LocalizationService : ILocalizationService
    {
        public string Get(string key) =>
            LocalizationProvider.Instance[key];

        public string LocalOrFallback(string key, string serverMessage, string fallbackKey)
        {
            if (!string.IsNullOrWhiteSpace(serverMessage)) return serverMessage;

            var primary = Get(key);
            return string.IsNullOrWhiteSpace(primary) ? Get(fallbackKey) : primary;
        }
    }
}
