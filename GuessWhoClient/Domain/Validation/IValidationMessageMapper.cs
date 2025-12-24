namespace GuessWhoClient.Domain.Validation
{
    public interface IValidationMessageMapper
    {
        string ToMessage(string validationKey);
    }
}
