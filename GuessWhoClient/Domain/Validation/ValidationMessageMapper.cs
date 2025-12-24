using GuessWhoClient.Globalization;

namespace GuessWhoClient.Domain.Validation
{
    public sealed class ValidationMessageMapper : IValidationMessageMapper
    {
        private readonly ILocalizationService localizationService; 

        public ValidationMessageMapper(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }

        public string ToMessage(string validationKey)
        {
            return localizationService.LocalOrFallback(
                validationKey,
                "Invalid input",
                "UiGenericValidationError");
        }
    }
}
