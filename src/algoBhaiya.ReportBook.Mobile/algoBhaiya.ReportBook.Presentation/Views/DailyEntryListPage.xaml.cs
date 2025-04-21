using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryListPage : ContentPage
{
    private readonly DailyEntryListViewModel _viewModel;
    public DailyEntryListPage(DailyEntryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    private async void OnTodayCalendarClicked(object sender, EventArgs e)
    {
        DateTime formDate = DateTime.Today;
        await _viewModel.OpenEntryAsync(formDate);
    }

    private async void OnDateCalendarClicked(object sender, EventArgs e)
    {
        var selected = await DatePickerDialog();
        if (selected != null)
        {
            DateTime formDate = selected ?? throw new ArgumentNullException(nameof(selected));
            await _viewModel.OpenEntryAsync(formDate);
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
            if (BindingContext is DailyEntryListViewModel vm)
            {
                await vm.LoadEntriesMonthlyAsync(year, month);
            }
        }

        await Navigation.PopModalAsync();
    }

    private async Task<DateTime?> DatePickerDialog()
    {
        var result = await Application.Current.MainPage.DisplayPromptAsync(
            "Select Date", "Enter date in yyyy-MM-dd format", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
        return DateTime.TryParse(result, out var dt) ? dt : null;
    }
}