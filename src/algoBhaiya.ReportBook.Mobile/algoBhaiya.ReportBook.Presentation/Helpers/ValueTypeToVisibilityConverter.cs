using System.Globalization;

namespace algoBhaiya.ReportBook.Presentation.Helpers
{
    public class ValueTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueType = value?.ToString()?.ToLower();
            string param = parameter?.ToString()?.ToLower();

            if (param == "numeric")
                return valueType == "int" || valueType == "double";

            if (param == "bool")
                return valueType == "bool";

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
