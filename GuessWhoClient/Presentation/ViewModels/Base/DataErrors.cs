using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace GuessWhoClient.Presentation.ViewsModels.Base
{
    public sealed class DataErrors
    {
        private readonly Dictionary<string, List<string>> errors =
            new Dictionary<string, List<string>>(StringComparer.Ordinal);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => errors.Count > 0;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return Array.Empty<string>();
            }

            if (errors.TryGetValue(propertyName, out List<string> propertyErrors))
            {
                return propertyErrors;
            }

            return Array.Empty<string>();
        }

        public void ReplaceAllErrors(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        {
            ClearAllErrorsInternal();

            if (errors == null || errors.Count == 0)
            {
                return;
            }

            foreach (var pair in errors)
            {
                string propertyName = pair.Key ?? string.Empty;
                IReadOnlyList<string> messages = pair.Value ?? Array.Empty<string>();

                for (int index = 0; index < messages.Count; index++)
                {
                    AddErrorInternal(propertyName, messages[index]);
                }

                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        private void AddErrorInternal(string propertyName, string errorMessage)
        {
            if (!errors.TryGetValue(propertyName, out List<string> propertyErrors))
            {
                propertyErrors = new List<string>();
                errors[propertyName] = propertyErrors;
            }

            if (!propertyErrors.Contains(errorMessage))
            {
                propertyErrors.Add(errorMessage);
            }
        }

        private void ClearAllErrorsInternal()
        {
            if (errors.Count == 0)
            {
                return;
            }

            string[] keys = new string[errors.Keys.Count];
            errors.Keys.CopyTo(keys, 0);

            errors.Clear();

            for (int index = 0; index < keys.Length; index++)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(keys[index]));
            }
        }
    }
}
