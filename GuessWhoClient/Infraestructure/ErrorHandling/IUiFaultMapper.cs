namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public interface IUiFaultMapper
    {
        bool TryMap(string faultCode, out string uiKey);
    }
}
