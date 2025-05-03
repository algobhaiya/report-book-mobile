using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Views;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class AppShell : Shell
    {
        private bool _isInitialized = false;          

        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            Routing.RegisterRoute(nameof(MonthlyTargetPage), typeof(MonthlyTargetPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SwitchProfilePage), typeof(SwitchProfilePage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_isInitialized)
            {
                if (BindingContext is AppShellViewModel vm)
                {
                    try
                    {
                        await vm.LoadUserNameAsync(); // Only after page fully loaded
                        _isInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                    }
                }
            }
        }
    }
}
