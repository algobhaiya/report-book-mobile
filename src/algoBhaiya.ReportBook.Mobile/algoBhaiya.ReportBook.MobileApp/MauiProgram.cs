using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Infrastructure.Data.Repositories;
using algoBhaiya.ReportBook.Presentation.Helpers;
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

            // Register the generic repository for scoped lifetime
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            builder.Services.AddScoped<IDailyEntryRepository, DailyEntryRepository>();
            builder.Services.AddScoped<IMonthlyTargetRepository, MonthlyTargetRepository>();

            // Register AppShell as a singleton
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddSingleton<AppShell>();

            // Register your services, view models, and pages here
            builder.Services.AddSingleton<MainPage>();

            // Register pages and view models
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddSingleton<NavigationDataService>();

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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
