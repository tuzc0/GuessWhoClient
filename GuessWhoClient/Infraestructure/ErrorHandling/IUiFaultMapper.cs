namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public interface IUiFaultMapper
    {
        UiKeyMapping Map(string faultCode);
    }
}
