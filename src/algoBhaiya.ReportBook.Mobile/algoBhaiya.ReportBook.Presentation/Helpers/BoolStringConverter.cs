
using System.Globalization;

namespace algoBhaiya.ReportBook.Presentation.Helpers
{
    public class BoolStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return bool.TryParse(value?.ToString(), out var result) && result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

}
