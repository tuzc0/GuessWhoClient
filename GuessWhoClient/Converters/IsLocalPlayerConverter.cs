using System;
using System.Globalization;
using System.Windows.Data;

namespace GuessWhoClient.Converters
{
    public sealed class IsLocalPlayerConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            if (!(values[0] is long playerUserId) ||
                !(values[1] is long currentUserId))
            {
                return false;
            }

            return playerUserId == currentUserId;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
