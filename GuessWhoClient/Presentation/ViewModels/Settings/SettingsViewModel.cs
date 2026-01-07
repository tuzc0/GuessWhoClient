using ClassLibraryGuessWho.Properties.Localization;
using GuessWhoClient.Globalization;
using GuessWhoClient.Presentation.ViewsModels.Base;
using System;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewModels.Settings
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private const string CULTURE_EN_US = "en-US";
        private const string CULTURE_ES_MX = "es-MX";
        private const string CULTURE_FR_FR = "fr-FR";
        private const string CULTURE_IT_IT = "it-IT";

        private readonly string originalCultureName;

        private bool isEnglishSelected;
        private bool isSpanishSelected;
        private bool isFrenchSelected;
        private bool isItalianSelected;

        public SettingsViewModel()
        {
            originalCultureName = Common.Culture?.Name ?? string.Empty;

            ApplyCommand = new RelayCommand(Apply, CanApply);
            CancelCommand = new RelayCommand(Cancel, () => true);

            SetSelectionFromCurrentCulture();
        }

        public Action RequestClose { get; set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public bool IsEnglishSelected
        {
            get => isEnglishSelected;
            set
            {
                if (isEnglishSelected == value) return;
                isEnglishSelected = value;
                OnPropertyChanged();
                RaiseCommands();
            }
        }

        public bool IsSpanishSelected
        {
            get => isSpanishSelected;
            set
            {
                if (isSpanishSelected == value) return;
                isSpanishSelected = value;
                OnPropertyChanged();
                RaiseCommands();
            }
        }

        public bool IsFrenchSelected
        {
            get => isFrenchSelected;
            set
            {
                if (isFrenchSelected == value) return;
                isFrenchSelected = value;
                OnPropertyChanged();
                RaiseCommands();
            }
        }

        public bool IsItalianSelected
        {
            get => isItalianSelected;
            set
            {
                if (isItalianSelected == value) return;
                isItalianSelected = value;
                OnPropertyChanged();
                RaiseCommands();
            }
        }

        private void Apply()
        {
            string cultureName = ResolveSelectedCultureName();
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return;
            }

            LocalizationProvider.Instance.ChangeCulture(cultureName);
            RequestClose?.Invoke();
        }

        private bool CanApply()
        {
            string cultureName = ResolveSelectedCultureName();
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return false;
            }

            string current = Common.Culture?.Name ?? string.Empty;
            return !string.Equals(current, cultureName, StringComparison.Ordinal);
        }

        private void Cancel()
        {
            if (!string.IsNullOrWhiteSpace(originalCultureName))
            {
                LocalizationProvider.Instance.ChangeCulture(originalCultureName);
            }

            RequestClose?.Invoke();
        }

        private string ResolveSelectedCultureName()
        {
            if (IsEnglishSelected) return CULTURE_EN_US;
            if (IsSpanishSelected) return CULTURE_ES_MX;
            if (IsFrenchSelected) return CULTURE_FR_FR;
            if (IsItalianSelected) return CULTURE_IT_IT;
            return string.Empty;
        }

        private void SetSelectionFromCurrentCulture()
        {
            string current = Common.Culture?.Name ?? string.Empty;

            IsEnglishSelected = string.Equals(current, CULTURE_EN_US, StringComparison.Ordinal);
            IsSpanishSelected = string.Equals(current, CULTURE_ES_MX, StringComparison.Ordinal);
            IsFrenchSelected = string.Equals(current, CULTURE_FR_FR, StringComparison.Ordinal);
            IsItalianSelected = string.Equals(current, CULTURE_IT_IT, StringComparison.Ordinal);

            if (!IsEnglishSelected && !IsSpanishSelected && !IsFrenchSelected && !IsItalianSelected)
            {
                IsEnglishSelected = true;
            }
        }

        private void RaiseCommands()
        {
            if (ApplyCommand is RelayCommand relay)
            {
                relay.RaiseCanExecuteChanged();
            }
        }
    }
}
