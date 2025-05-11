using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Infrastructure.Data;
using algoBhaiya.ReportBook.Infrastructure.Data.Repositories;
using algoBhaiya.ReportBook.MobileApp.Services;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBook.Presentation.Services;
using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBook.Presentation.Views;
using algoBhaiya.ReportBooks.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SQLite;


namespace algoBhaiya.ReportBook.MobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp() 
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register the SQLite connection
            builder.Services.AddSingleton(db =>
            {
                // Configure Database
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ReportBookClient.db");
                return new SQLiteAsyncConnection(dbPath);
            });

            builder.Services.AddSingleton<DatabaseInitializer>();

            // Register the generic repository for scoped lifetime
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            builder.Services.AddScoped<IDailyEntryRepository, DailyEntryRepository>();
            builder.Services.AddScoped<IMonthlyTargetRepository, MonthlyTargetRepository>();
            builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();
            builder.Services.AddScoped<ISeedDataService, SeedDataService>();

            builder.Services.AddSingleton<IAppNavigator, AppNavigator>();

            // Register AppShell as a singleton
            builder.Services.AddTransient<AppShellViewModel>();
            builder.Services.AddTransient<AppShell>();

            // Register your services, view models, and pages here
            builder.Services.AddSingleton<NavigationDataService>();
            
            // Register pages and view models
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddTransient<SwitchProfilePageViewModel>();
            builder.Services.AddTransient<SwitchProfilePage>();

            builder.Services.AddSingleton<DailyEntryViewModel>();
            builder.Services.AddTransient<DailyEntryPage>();

            builder.Services.AddTransient<FieldTemplateDetailViewModel>();
            builder.Services.AddTransient<FieldTemplateDetailPage>();

            builder.Services.AddTransient<FieldTemplatePage>();

            builder.Services.AddTransient<FieldUnitAddEditViewModel>();
            builder.Services.AddTransient<FieldUnitAddEditPage>();
            
            builder.Services.AddTransient<FieldUnitPage>();

            builder.Services.AddTransient<MonthlyTargetViewModel>();
            builder.Services.AddTransient<MonthlyTargetPage>();

            builder.Services.AddTransient<DailyEntryListViewModel>();
            builder.Services.AddTransient<DailyEntryListPage>();

            builder.Services.AddTransient<MonthlySummaryViewModel>();
            builder.Services.AddTransient<MonthlySummaryPage>();

            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SettingsPage>();

            builder.Services.AddSingleton<HelpPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
