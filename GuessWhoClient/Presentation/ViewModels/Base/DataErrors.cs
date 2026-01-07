using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
                if (errors.Count == 0)
                {
                    return Array.Empty<string>();
                }

                return errors.Values.SelectMany(x => x).ToList();
            }

            return errors.TryGetValue(propertyName, out List<string> propertyErrors)
                ? (IEnumerable)propertyErrors
                : Array.Empty<string>();
        }

        public string FirstErrorOrEmpty(string propertyName)
        {
            IEnumerable list = GetErrors(propertyName);

            foreach (object item in list)
            {
                if (item is string message && !string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }

            return string.Empty;
        }

        public void ReplaceAllErrors(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        {
            ClearAllErrorsInternal();

            if (errors == null || errors.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, IReadOnlyList<string>> pair in errors)
            {
                string propertyName = pair.Key ?? string.Empty;
                IReadOnlyList<string> messages = pair.Value ?? Array.Empty<string>();

                for (int index = 0; index < messages.Count; index++)
                {
                    string message = messages[index];

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        AddErrorInternal(propertyName, message);
                    }
                }

                RaiseErrorsChanged(propertyName);
            }
        }

        public void ClearAllErrors()
        {
            ClearAllErrorsInternal();
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

            string[] keys = errors.Keys
                .Select(k => k ?? string.Empty)
                .ToArray();

            errors.Clear();

            for (int index = 0; index < keys.Length; index++)
            {
                RaiseErrorsChanged(keys[index]);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
