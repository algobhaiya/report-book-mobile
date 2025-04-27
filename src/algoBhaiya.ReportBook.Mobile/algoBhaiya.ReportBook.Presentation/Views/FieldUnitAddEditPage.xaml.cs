using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitAddEditPage : ContentPage
{
    public TaskCompletionSource<int?> ResultSource { get; } = new();
    public FieldUnitAddEditPage(FieldUnitAddEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        viewModel.onModalClose = OnModalClosed;
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