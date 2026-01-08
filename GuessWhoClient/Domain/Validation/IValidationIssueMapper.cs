namespace GuessWhoClient.Domain.Validation
{
    public interface IValidationIssueMapper
    {
        ValidationIssueMapping Map(string issueKey);
    }
}