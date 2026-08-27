using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Views;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class AppShell : Shell
    {
        private bool _isInitialized;
        private bool _hasShownStartupStreakLossThisSession;

        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            Navigated += OnShellNavigated;

            Routing.RegisterRoute(nameof(MonthlySummaryPage), typeof(MonthlySummaryPage));
            Routing.RegisterRoute(nameof(DailyEntryPage), typeof(DailyEntryPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(SwitchProfilePage), typeof(SwitchProfilePage));
            UpdatePageTitle();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            UpdatePageTitle();

            await Task.Yield();

            if (!_isInitialized)
            {
                if (BindingContext is AppShellViewModel vm)
                {
                    _isInitialized = true;
                    _ = InitializeShellAsync(vm);
                }
            }
            else
            {
                await RefreshStreakAsync();
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

        private async Task RefreshStreakAsync()
        {
            if (BindingContext is not AppShellViewModel vm)
            {
                return;
            }

            try
            {
                await vm.RefreshStreakAsync();
            }
            catch
            {
            }
        }

        private async Task HandleStartupStreakLossAsync(AppShellViewModel vm)
        {
            if (_hasShownStartupStreakLossThisSession)
            {
                return;
            }

            var result = await vm.RefreshStartupStreakAsync();

            if (result.IsStartupLoss)
            {
                _hasShownStartupStreakLossThisSession = true;
                await vm.ShowStartupStreakLossAsync();
            }
        }

        private async Task InitializeShellAsync(AppShellViewModel vm)
        {
            try
            {
                await HandleStartupStreakLossAsync(vm);
                await vm.LoadUserNameAsync();
            }
            catch
            {
            }
        }
    }
}
