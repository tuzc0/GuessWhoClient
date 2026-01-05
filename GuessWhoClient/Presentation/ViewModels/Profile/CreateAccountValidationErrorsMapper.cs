using GuessWhoClient.Globalization;
using GuessWhoCore.Validation;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Presentation.ViewModels.Profile
{
    public interface ICreateAccountValidationErrorsMapper
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> Map(IReadOnlyList<ValidationError> errors);
    }

    public sealed class CreateAccountValidationErrorsMapper : ICreateAccountValidationErrorsMapper
    {
        private readonly ILocalizationService localizationService;
        private readonly IValidationIssueMapper issueMapper;

        public CreateAccountValidationErrorsMapper(
            ILocalizationService localizationService,
            IValidationIssueMapper issueMapper)
        {
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.issueMapper = issueMapper ?? throw new ArgumentNullException(nameof(issueMapper));
        }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Map(IReadOnlyList<ValidationError> errors)
        {
            var mapped = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            if (errors == null || errors.Count == 0)
            {
                return mapped;
            }

            for (int index = 0; index < errors.Count; index++)
            {
                ValidationError error = errors[index];

                if (error == null || string.IsNullOrWhiteSpace(error.Key))
                {
                    continue;
                }

                ValidationIssueMapping mapping = issueMapper.Map(error.Key);

                if (!mapping.IsMapped)
                {
                    continue;
                }

                string message = localizationService.Get(mapping.MessageKey);

                if (!mapped.TryGetValue(mapping.PropertyName, out IReadOnlyList<string> existing))
                {
                    mapped[mapping.PropertyName] = new List<string> { message };
                    continue;
                }

                var list = existing as List<string> ?? new List<string>(existing);

                if (!list.Contains(message))
                {
                    list.Add(message);
                }

                mapped[mapping.PropertyName] = list;
            }

            return mapped;
        }
    }
}
