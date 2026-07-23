using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryPage : ContentPage
{
    private bool _isInitialized = false;
    private bool _isNavigatingDate = false;

    public DailyEntryPage(DailyEntryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;		
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            if (BindingContext is DailyEntryViewModel vm)
            {
                try
                {
                    await vm.LoadFieldsAsync(); // Only after page fully loaded
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
