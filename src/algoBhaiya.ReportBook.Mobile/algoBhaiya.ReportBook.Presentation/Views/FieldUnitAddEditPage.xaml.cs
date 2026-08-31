using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitAddEditPage : ContentPage
{
    public TaskCompletionSource<int?> ResultSource { get; } = new();
    private bool _isClosing;

    public FieldUnitAddEditPage(FieldUnitAddEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        viewModel.onModalClose = OnModalClosed;
	}

    private void OnModalClosed()
    {
        CloseModal();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        CloseModal();
    }

    private void CloseModal()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        ResultSource.TrySetResult(null);
    }
}
