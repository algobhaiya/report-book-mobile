using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitAddEditPage : ContentPage
{
	public FieldUnitAddEditPage(FieldUnitAddEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}