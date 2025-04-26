using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldTemplateDetailPage : ContentPage
{
    public TaskCompletionSource<int?> ResultSource { get; } = new();
    public FieldTemplateDetailPage(FieldTemplateDetailViewModel viewModel)
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