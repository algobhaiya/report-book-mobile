using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryPage : ContentPage
{
    private bool _isInitialized = false;

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
}