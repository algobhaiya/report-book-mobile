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
            Navigated += OnShellNavigated;

            Routing.RegisterRoute(nameof(MonthlySummaryPage), typeof(MonthlySummaryPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(SwitchProfilePage), typeof(SwitchProfilePage));
            UpdatePageTitle();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            UpdatePageTitle();

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

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            UpdatePageTitle();
        }

        private void UpdatePageTitle()
        {
            if (BindingContext is not AppShellViewModel vm)
            {
                return;
            }

            var pageTitle = CurrentPage?.Title;

            if (string.IsNullOrWhiteSpace(pageTitle))
            {
                pageTitle = CurrentItem?.CurrentItem?.CurrentItem?.Title ?? CurrentItem?.Title;
            }

            vm.UpdatePageTitle(pageTitle);
        }
    }
}
