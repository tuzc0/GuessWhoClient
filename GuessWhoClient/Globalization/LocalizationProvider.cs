using ClassLibraryGuessWho.Properties.Localization;
using System.ComponentModel;
using System.Globalization;

namespace GuessWhoClient.Globalization
{
    public class LocalizationProvider : INotifyPropertyChanged
    {
        private static readonly LocalizationProvider instance = new LocalizationProvider();
        public static LocalizationProvider Instance => instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string key] =>
            Common.ResourceManager.GetString(key, Common.Culture ?? CultureInfo.CurrentUICulture) ?? $"!{key}!";

        public void ChangeCulture(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            Common.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
