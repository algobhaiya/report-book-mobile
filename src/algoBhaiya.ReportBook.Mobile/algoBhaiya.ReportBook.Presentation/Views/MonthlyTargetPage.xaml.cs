using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Helpers;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlyTargetPage : ContentPage
{
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized = false;
	public MonthlyTargetPage(
        MonthlyTargetViewModel viewModel,
        NavigationDataService navDataService)
	{
		InitializeComponent();
		BindingContext = viewModel;
        _navDataService = navDataService;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not MonthlyTargetViewModel vm)
        {
            return;
        }

        if (!_isInitialized)
        {
            try
            {
                await vm.LoadTargetsAsync(DateTime.Today.Year, DateTime.Today.Month);
                _isInitialized = true;
            }
            catch (Exception)
            {
            }
            return;
        }

        if (_navDataService.Get<bool>(AppConstants.FieldTemplate.Action_RefreshOnReturn))
        {
            try
            {
                await vm.RefreshTargetsAsync();
            }
            finally
            {
                _navDataService.Remove(AppConstants.FieldTemplate.Action_RefreshOnReturn);
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
            if (BindingContext is MonthlyTargetViewModel vm)
            {
                await vm.LoadTargetsAsync(year, month);
            }
        }

        await Navigation.PopModalAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        if (BindingContext is MonthlyTargetViewModel vm)
        {
            await vm.RefreshTargetsAsync();
        }
    }

}
