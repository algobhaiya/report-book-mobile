using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DailyEntryPage : ContentPage
{
	public DailyEntryPage(DailyEntryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
		viewModel.LoadCommand.Execute(null);
    }
}