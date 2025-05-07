using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlySummaryPage : ContentPage
{
	public MonthlySummaryPage(MonthlySummaryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
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