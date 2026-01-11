namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public interface IMatchUiKeyResolver
    {
        string ResolveFaultOrServerKey(string faultCode, string serverMessageKey, string fallbackKey);

        string ResolveCodeOrFallback(string code, string fallbackKey);
    }
}
