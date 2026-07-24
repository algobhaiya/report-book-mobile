using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Helpers;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryListPage : ContentPage
{
    private readonly DailyEntryListViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized = false;
    private bool _isOpeningMonthlySummary = false;

    public DailyEntryListPage(DailyEntryListViewModel viewModel, IServiceProvider serviceProvider, NavigationDataService navDataService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        _navDataService = navDataService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            if (BindingContext is DailyEntryListViewModel vm)
            {
                try
                {
                    await vm.RefreshDailyEntriesAsync(); // Only after page fully loaded
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }
        }
        else if (_navDataService.Get<bool>(AppConstants.DailyEntry.Action_RefreshListOnReturn))
        {
            try
            {
                await _viewModel.RefreshDailyEntriesAsync();
            }
            catch (Exception ex)
            {
                // Log exception
            }
            finally
            {
                _navDataService.Remove(AppConstants.DailyEntry.Action_RefreshListOnReturn);
            }
        }
    }

    private async void OnTodayCalendarClicked(object sender, EventArgs e)
    {
        DateTime formDate = DateTime.Today;
        await _viewModel.OpenEntryAsync(formDate);
    }

    private async void OnDateCalendarClicked(object sender, EventArgs e)
    {
        var popup = new DatePickerPopup();
        await Navigation.PushModalAsync(popup);
        var selected = await popup.ResultSource.Task;

        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }

        if (selected.HasValue)
        {
            await Task.Yield();
            await _viewModel.OpenEntryAsync(selected.Value);
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

    private async void OnMonthlySummaryClicked(object sender, EventArgs e)
    {
        if (_isOpeningMonthlySummary)
        {
            return;
        }

        try
        {
            _isOpeningMonthlySummary = true;

            var monthlySummaryPage = _serviceProvider.GetRequiredService<MonthlySummaryPage>();
            if (monthlySummaryPage.BindingContext is MonthlySummaryViewModel monthlySummaryVm)
            {
                await monthlySummaryVm.LoadDataAsync(_viewModel.SelectedMonthDate.Year, _viewModel.SelectedMonthDate.Month);
            }

            await Shell.Current.Navigation.PushAsync(monthlySummaryPage);
        }
        finally
        {
            _isOpeningMonthlySummary = false;
        }
    }

}

