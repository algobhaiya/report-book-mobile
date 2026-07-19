using algoBhaiya.ReportBook.Presentation.ViewModels;
using System.Globalization;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class SwitchProfilePage : ContentPage
{
    private bool _isInitialized;

	public SwitchProfilePage(SwitchProfilePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;

        if (BindingContext is SwitchProfilePageViewModel vm)
        {
            try
            {
                await Task.Yield();
                await vm.InitializeAsync();
            }
            catch
            {
                // Keep the modal usable even if loading fails.
            }
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}

public class UserInitialsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return "?";

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpperInvariant();

        return string.Concat(parts[0][0].ToString().ToUpperInvariant(), parts[1][0].ToString().ToUpperInvariant());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ProfileSelectionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        var currentId = values[0] is byte c ? c : (byte)0;
        var profileId = values[1] is byte p ? p : (byte)0;
        return currentId == profileId;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ProfileAccentColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return new SolidColorBrush(Colors.Transparent);

        var currentId = values[0] is byte c ? c : (byte)0;
        var profileId = values[1] is byte p ? p : (byte)0;
        var selected = currentId == profileId;
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        if (parameter?.ToString() == "stroke")
            return new SolidColorBrush(selected
                ? Color.FromArgb(isDark ? "#60A5FA" : "#2563EB")
                : Color.FromArgb(isDark ? "#454B54" : "#E5E7EB"));

        return selected
            ? Color.FromArgb(isDark ? "#172554" : "#EFF6FF")
            : Color.FromArgb(isDark ? "#23272E" : "#F9FAFB");
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
