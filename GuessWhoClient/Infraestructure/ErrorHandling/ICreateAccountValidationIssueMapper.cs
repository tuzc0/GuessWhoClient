using GuessWhoCore.Validation;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public interface ICreateAccountValidationIssueMapper
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> Map(IReadOnlyList<ValidationError> errors);
    }
}
