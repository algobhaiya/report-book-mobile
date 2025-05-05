using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MonthlySummaryPage : ContentPage
{
	public MonthlySummaryPage(MonthlySummaryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}