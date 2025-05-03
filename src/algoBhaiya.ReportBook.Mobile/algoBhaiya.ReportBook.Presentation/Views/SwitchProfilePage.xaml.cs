using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class SwitchProfilePage : ContentPage
{
	public SwitchProfilePage(SwitchProfilePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}