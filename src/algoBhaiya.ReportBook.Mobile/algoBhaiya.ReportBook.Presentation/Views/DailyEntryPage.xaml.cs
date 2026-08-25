using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;
using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryPage : ContentPage
{
    private bool _isInitialized = false;
    private bool _isNavigatingDate = false;
    private readonly Helpers.NavigationDataService _navDataService;

    public DailyEntryPage(DailyEntryViewModel viewModel, Helpers.NavigationDataService navDataService)
	{
		InitializeComponent();
        BindingContext = viewModel;		
        _navDataService = navDataService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DailyEntryViewModel vm &&
            _navDataService.Get<bool>(AppConstants.DailyEntry.Action_InvalidateCache))
        {
            vm.InvalidateCache();
            _navDataService.Remove(AppConstants.DailyEntry.Action_InvalidateCache);
        }

        if (!_isInitialized)
        {
            if (BindingContext is DailyEntryViewModel dailyVm)
            {
                try
                {
                    var selectedDate = _navDataService.Get<DateTime>(AppConstants.DailyEntry.Item_SelectedDate);
                    if (selectedDate == default)
                    {
                        selectedDate = DateTime.Today;
                    }
                    await dailyVm.LoadFieldsForDateAsync(selectedDate); // Uses cache when possible, falls back to full load
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }
        }
    }

    private async Task NavigateByDaysAsync(int offset)
    {
        if (_isNavigatingDate || BindingContext is not DailyEntryViewModel vm)
        {
            return;
        }

        try
        {
            _isNavigatingDate = true;
            await vm.LoadFieldsForDateAsync(vm.FormDate.Date.AddDays(offset));
        }
        catch (Exception)
        {
        }
        finally
        {
            _isNavigatingDate = false;
        }
    }

    private async void OnPreviousDayClicked(object sender, EventArgs e)
    {
        await NavigateByDaysAsync(-1);
    }

    private async void OnNextDayClicked(object sender, EventArgs e)
    {
        await NavigateByDaysAsync(1);
    }
}
