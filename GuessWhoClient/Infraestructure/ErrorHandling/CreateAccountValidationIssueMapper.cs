using GuessWhoClient.Globalization;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoCore.Validation;
using System;
using System.Collections.Generic;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class CreateAccountValidationIssueMapper : ICreateAccountValidationIssueMapper
    {
        private readonly ILocalizationService localizationService;
        private readonly IValidationIssueMapper validationIssueMapper;

        public CreateAccountValidationIssueMapper(
            ILocalizationService localizationService,
            IValidationIssueMapper validationIssueMapper)
        {
            this.localizationService = localizationService ?? 
                throw new ArgumentNullException(nameof(localizationService));
            this.validationIssueMapper = validationIssueMapper ?? 
                throw new ArgumentNullException(nameof(validationIssueMapper));
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

                ValidationIssueMapping mapping = validationIssueMapper.Map(error.Key);

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
