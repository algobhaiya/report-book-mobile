using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlyTargetPage : ContentPage
{
	public MonthlyTargetPage(
        MonthlyTargetViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void OnMonthCalendarClicked(object sender, EventArgs e)
    {
        var popup = new YearMonthPickerPopup();
        await Navigation.PushModalAsync(popup);
        var result = await popup.ResultSource.Task;

        if (result.HasValue)
        {
            var (year, month) = result.Value;
            if (BindingContext is MonthlyTargetViewModel vm)
            {
                await vm.LoadTargetsAsync(year, month);
            }
        }

        await Navigation.PopModalAsync();
    }
}