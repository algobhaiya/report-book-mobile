using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
