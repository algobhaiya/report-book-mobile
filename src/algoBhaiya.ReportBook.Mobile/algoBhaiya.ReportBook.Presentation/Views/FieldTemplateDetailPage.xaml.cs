using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldTemplateDetailPage : ContentPage
{
    public TaskCompletionSource<int?> ResultSource { get; } = new();
    private bool _isInitialized = false;
    private bool _isClosing = false;

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
        CloseAsync();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        CloseAsync();
    }

    private void CloseAsync()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        ResultSource.TrySetResult(null);
    }
}
