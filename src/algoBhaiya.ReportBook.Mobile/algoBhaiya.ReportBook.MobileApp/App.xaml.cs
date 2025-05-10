using algoBhaiya.ReportBook.Core.Interfaces;

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
            
            NavigateToUserPage();

            SeedInitialDataAsync();

            CleanUpData();
        }

        private void NavigateToUserPage()
        {
            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
                _navigator.NavigateToMainShell();
            else
                _navigator.NavigateToLogin();
        }

        private void CleanUpData()
        {
            // Run cleanup task
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
                catch (Exception ex)
                {
                    // Optional: log error
                }
            });
        }

        private void SeedInitialDataAsync()
        {
            // Run cleanup task
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
                catch (Exception ex)
                {
                    // Optional: log error
                }
            });
            
        }
    }

}
