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

            InitializeDatabase();

            NavigateToUserPage();
            RefreshCurrentUserStreakAsync();

            SeedInitialDataAsync();

            CleanUpData();
        }

        private void InitializeDatabase()
        {
            var initializer = _serviceProvider.GetRequiredService<DatabaseInitializer>();
            Task.Run(async () => await initializer.InitializeAsync()).Wait();
        }

        private void NavigateToUserPage()
        {
            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
                _navigator.NavigateToMainShell();
            else
                _navigator.NavigateToLogin();
        }

        protected override void OnResume()
        {
            base.OnResume();
            RefreshCurrentUserStreakAsync();
        }

        private void RefreshCurrentUserStreakAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                    var streakService = _serviceProvider.GetService<ITrackingStreakService>();
                    if (streakService != null)
                    {
                        await streakService.RefreshForCurrentDayAsync((byte)Preferences.Get("CurrentUserId", 0));
                    }
                }
                catch
                {
                }
            });
        }

        private void CleanUpData()
        {
            Task.Run(async () =>
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
            });
        }

        private void SeedInitialDataAsync()
        {
            Task.Run(async () =>
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
            });
        }
    }
}
