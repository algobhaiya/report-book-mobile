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

    private async void OnCalendarClicked(object sender, EventArgs e)
    {
        var selected = await DatePickerDialog();
        if (selected != null)
        {
            DateTime formDate = selected ?? throw new ArgumentNullException(nameof(selected));
            await _viewModel.OpenEntryAsync(formDate);
        }
    }

    private async Task<DateTime?> DatePickerDialog()
    {
        var result = await Application.Current.MainPage.DisplayPromptAsync(
            "Select Date", "Enter date in yyyy-MM-dd format", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
        return DateTime.TryParse(result, out var dt) ? dt : null;
    }

}