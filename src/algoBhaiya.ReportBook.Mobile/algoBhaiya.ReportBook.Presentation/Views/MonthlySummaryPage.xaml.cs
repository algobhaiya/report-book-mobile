using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlySummaryPage : ContentPage, IQueryAttributable
{
    private bool _isInitialized = false;
    private int? _pendingYear;
    private int? _pendingMonth;

    public MonthlySummaryPage(MonthlySummaryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            return;
        }

        if (BindingContext is MonthlySummaryViewModel vm)
        {
            _isInitialized = true;
            if (_pendingYear.HasValue && _pendingMonth.HasValue)
            {
                await vm.LoadDataAsync(_pendingYear.Value, _pendingMonth.Value);
                _pendingYear = null;
                _pendingMonth = null;
                return;
            }

            if (!vm.HasLoadedMonth)
            {
                await vm.LoadDataAsync(DateTime.Today.Year, DateTime.Today.Month);
            }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("year", out var yearValue) && int.TryParse(yearValue?.ToString(), out var year) &&
            query.TryGetValue("month", out var monthValue) && int.TryParse(monthValue?.ToString(), out var month))
        {
            _pendingYear = year;
            _pendingMonth = month;
        }
    }

    private async void OnMonthCalendarClicked(object sender, EventArgs e)
    {
        var popup = new YearMonthPickerPopup();
        await Navigation.PushModalAsync(popup);
        var result = await popup.ResultSource.Task;

        if (result.HasValue)
        {
            var (year, month) = result.Value;
            if (BindingContext is MonthlySummaryViewModel vm)
            {
                await vm.LoadDataAsync(year, month);
            }
        }

        await Navigation.PopModalAsync();
    }
}
