using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryListPage : ContentPage
{
    public DailyEntryListPage(DailyEntryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCalendarClicked(object sender, EventArgs e)
    {
        var selected = await DatePickerDialog();
        if (selected != null)
        {
            var vm = BindingContext as DailyEntryListViewModel;
            //await vm.OpenEntryCommand.ExecuteAsync(selected.Value);
        }
    }

    private async Task<DateTime?> DatePickerDialog()
    {
        var result = await Application.Current.MainPage.DisplayPromptAsync(
            "Select Date", "Enter date in yyyy-MM-dd format", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
        return DateTime.TryParse(result, out var dt) ? dt : null;
    }

}