using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Infrastructure.Data;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _navigator;

        public App(
            IServiceProvider serviceProvider,
            IAppNavigator navigator)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _navigator = navigator;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(CreateStartupPage());
            _ = InitializeStartupAsync();
            return window;
        }

        private static Page CreateStartupPage()
        {
            var loadingIndicator = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var overlay = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                Children = { loadingIndicator }
            };

            var page = new ContentPage
            {
                Content = new Grid
                {
                    Children = { overlay }
                }
            };

            page.SetAppThemeColor(Page.BackgroundColorProperty, Color.FromArgb("#66000000"), Color.FromArgb("#99000000"));
            loadingIndicator.SetAppThemeColor(ActivityIndicator.ColorProperty, Colors.White, Color.FromArgb("#F9FAFB"));

            return page;
        }

        private async Task InitializeStartupAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await InitializeDatabaseAsync();
                    await SeedInitialDataAsync();
                    await CleanUpData();
                });
            }
            catch
            {
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(NavigateToUserPage);
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            var initializer = _serviceProvider.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();
        }

        private void NavigateToUserPage()
        {
            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
                _navigator.NavigateToMainShell();
            else
                _navigator.NavigateToLogin();
        }

        private async Task CleanUpData()
        {
            try
            {
                var dataRetentionService = _serviceProvider.GetService<IDataRetentionService>();
                if (dataRetentionService != null)
                {
                    await dataRetentionService.PerformIncrementalCleanupAsync();
                }
            }
            catch
            {
            }
        }

        private async Task SeedInitialDataAsync()
        {
            try
            {
                var seedingDataService = _serviceProvider.GetService<ISeedDataService>();
                if (seedingDataService != null)
                {
                    await seedingDataService.SeedDefaultUnitsAsync();
                }
            }
            catch
            {
            }
        }
    }
}
