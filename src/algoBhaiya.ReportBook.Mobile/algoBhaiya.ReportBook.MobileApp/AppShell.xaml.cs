using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Views;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            Routing.RegisterRoute(nameof(MonthlyTargetPage), typeof(MonthlyTargetPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            
        }
    }
}
