using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldTemplateDetailPage : ContentPage
{
    public TaskCompletionSource<int?> ResultSource { get; } = new();
    private bool _isInitialized = false;

    public FieldTemplateDetailPage(FieldTemplateDetailViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.onModalClose = OnModalClosed;        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {           
            if (BindingContext is FieldTemplateDetailViewModel vm)
            {
                try
                {
                    await vm.PopulateDataAsync(); // Only after page fully loaded
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }
        }
    }

    private void OnModalClosed()
    {       
        ResultSource.SetResult(null);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ResultSource.SetResult(null);
    }
}