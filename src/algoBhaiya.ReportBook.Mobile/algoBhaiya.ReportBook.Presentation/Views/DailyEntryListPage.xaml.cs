using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBook.Presentation.ViewModels;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryListPage : ContentPage
{
    private readonly DailyEntryListViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized = false;
    private bool _isOpeningMonthlySummary = false;
    private bool _isCelebrationVisible = false;

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
                    await RefreshShellHeaderAsync();
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
            var showCelebration = _navDataService.Get<bool>(AppConstants.DailyEntry.Action_ShowCompletionCelebration);
            try
            {
                await _viewModel.RefreshDailyEntriesAsync();
                await RefreshShellHeaderAsync();
                if (showCelebration)
                {
                    await ShowCelebrationAsync();
                }
            }
            catch (Exception ex)
            {
                // Log exception
            }
            finally
            {
                _navDataService.Remove(AppConstants.DailyEntry.Action_RefreshListOnReturn);
                _navDataService.Remove(AppConstants.DailyEntry.Action_ShowCompletionCelebration);
            }
        }
    }

    private static async Task RefreshShellHeaderAsync()
    {
        if (Shell.Current?.BindingContext is AppShellViewModel shellViewModel)
        {
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

    private async Task ShowCelebrationAsync()
    {
        if (_isCelebrationVisible || CelebrationOverlay == null)
        {
            return;
        }

        try
        {
            _isCelebrationVisible = true;
            CelebrationOverlay.IsVisible = true;
            CelebrationOverlay.Opacity = 0;
            CelebrationCard.Scale = 0.85;
            CelebrationCard.Opacity = 0;

            await Task.WhenAll(
                CelebrationOverlay.FadeTo(1, 180, Easing.CubicOut),
                CelebrationCard.FadeTo(1, 180, Easing.CubicOut),
                CelebrationCard.ScaleTo(1, 220, Easing.CubicOut));

            await Task.Delay(1800);

            await Task.WhenAll(
                CelebrationCard.FadeTo(0, 180, Easing.CubicIn),
                CelebrationOverlay.FadeTo(0, 200, Easing.CubicIn));

            CelebrationOverlay.IsVisible = false;
        }
        finally
        {
            _isCelebrationVisible = false;
        }
    }
}
