namespace GuessWhoClient.Infraestructure.ErrorHandling.Mapper
{
    public interface IFaultUiCatalog
    {
        string ResolveUiKey(string faultCode);
    }
}
