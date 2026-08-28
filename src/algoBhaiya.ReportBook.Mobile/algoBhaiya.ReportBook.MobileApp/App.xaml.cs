using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Infrastructure.Data;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class App : Application, IStartupInitializationService
    {
        private readonly IServiceProvider _serviceProvider;
        private static Task? _startupInitializationTask;

        public Task StartupInitializationTask => _startupInitializationTask ?? Task.CompletedTask;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(CreateInitialPage());
            _startupInitializationTask = InitializeStartupAsync();
            return window;
        }

        private Page CreateInitialPage()
        {
            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
            {
                return _serviceProvider.GetRequiredService<AppShell>();
            }

            return _serviceProvider.GetRequiredService<Presentation.Views.LoginPage>();
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
        }

        private async Task InitializeDatabaseAsync()
        {
            var initializer = _serviceProvider.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();
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
