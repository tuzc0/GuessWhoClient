using GuessWhoClient.Domain.Validation;
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
        private const string EMPTY = "";
        private const string LOCALIZATION_MISSING_PREFIX = "!";
        private const string LOCALIZATION_MISSING_SUFFIX = "!";

        private readonly ILocalizationService localizationService;
        private readonly IValidationIssueMapper issueMapper;

        public CreateAccountValidationErrorsMapper(
            ILocalizationService localizationService,
            IValidationIssueMapper issueMapper)
        {
            this.localizationService = localizationService ?? 
                throw new ArgumentNullException(nameof(localizationService));
            this.issueMapper = issueMapper ?? 
                throw new ArgumentNullException(nameof(issueMapper));
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

                string propertyName = mapping.PropertyName ?? EMPTY;

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    continue;
                }

                string message = GetLocalizedOrFallback(mapping.MessageKey);

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                if (!mapped.TryGetValue(propertyName, out IReadOnlyList<string> existing))
                {
                    mapped[propertyName] = new List<string> { message };
                    continue;
                }

                var list = existing as List<string> ?? new List<string>(existing);

                if (!list.Contains(message))
                {
                    list.Add(message);
                }

                mapped[propertyName] = list;
            }

            return mapped;
        }

        private string GetLocalizedOrFallback(string messageKey)
        {
            if (string.IsNullOrWhiteSpace(messageKey))
            {
                return EMPTY;
            }

            string localized = localizationService.Get(messageKey) ?? EMPTY;
            string missingMarker = string.Concat(LOCALIZATION_MISSING_PREFIX, messageKey, LOCALIZATION_MISSING_SUFFIX);

            return string.Equals(localized, missingMarker, StringComparison.Ordinal)
                ? messageKey
                : localized;
        }
    }
}
