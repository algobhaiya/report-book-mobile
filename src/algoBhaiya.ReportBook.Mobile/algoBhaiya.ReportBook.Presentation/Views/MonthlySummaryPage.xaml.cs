using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlySummaryPage : ContentPage
{
    private bool _isInitialized = false;

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
            if (!vm.HasLoadedMonth)
            {
                await vm.LoadDataAsync(DateTime.Today.Year, DateTime.Today.Month);
            }
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
