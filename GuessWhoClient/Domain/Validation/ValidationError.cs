using System.Windows.Input;

namespace GuessWhoClient.Domain.Validation
{
    public sealed class ValidationError
    {
        public string Key { get; }

        private ValidationError(string key)
        {
            Key = key ?? string.Empty;
        }

        public static ValidationError Create(string key)
        {
            return new ValidationError(key);
        }
    }
}
