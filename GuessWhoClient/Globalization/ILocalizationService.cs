namespace GuessWhoClient.Globalization
{
    public interface ILocalizationService
    {
        string Get(string key);
        string LocalOrFallback(string key, string serverMessage, string fallbackKey);
    }
}
