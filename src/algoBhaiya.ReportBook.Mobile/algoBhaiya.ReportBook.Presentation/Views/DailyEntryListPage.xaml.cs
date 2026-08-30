using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBook.Presentation.ViewModels;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryListPage : ContentPage
{
    private readonly DailyEntryListViewModel _viewModel;
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized = false;
    private bool _isOpeningMonthlySummary = false;

    public DailyEntryListPage(DailyEntryListViewModel viewModel, NavigationDataService navDataService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
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
                    await vm.RefreshDailyEntriesAsync();
                    await RefreshShellHeaderAsync();
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }

            return;
        } 
        else if (_navDataService.Get<bool>(AppConstants.DailyEntry.Action_RefreshListOnReturn))
        {
            try
            {
                await _viewModel.RefreshDailyEntriesAsync();
                await RefreshShellHeaderAsync();
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

    private static async Task RefreshShellHeaderAsync()
    {
        if (Shell.Current?.BindingContext is AppShellViewModel shellViewModel)
        {
            await shellViewModel.InitializeStartupAsync();
            await shellViewModel.RefreshStreakAsync();
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

            await Shell.Current.GoToAsync(
                nameof(MonthlySummaryPage),
                new Dictionary<string, object>
                {
                    ["year"] = _viewModel.SelectedMonthDate.Year,
                    ["month"] = _viewModel.SelectedMonthDate.Month
                });
        }
        finally
        {
            _isOpeningMonthlySummary = false;
        }
    }
}
