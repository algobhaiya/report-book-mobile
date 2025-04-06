using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlyTargetPage : ContentPage
{
	public MonthlyTargetPage(
        MonthlyTargetViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}