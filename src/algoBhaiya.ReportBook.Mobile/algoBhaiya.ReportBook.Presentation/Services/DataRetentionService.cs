using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    public class DataRetentionService : IDataRetentionService
    {
        private const string LastCleanupDateKey = "last_cleanup_date";
        private readonly IServiceProvider _serviceProvider;
        private readonly IDailyEntryRepository _dailyEntryRepository;

        public DataRetentionService(
            IServiceProvider serviceProvider,
            IDailyEntryRepository dailyEntryRepository)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException();
            _dailyEntryRepository = dailyEntryRepository ?? throw new ArgumentNullException();
        }
        
        public async Task PerformIncrementalCleanupAsync()
        {
            DateTime today = DateTime.UtcNow.Date;

            // Get last cleanup date (or default to today - 1 day)
            var lastCleanupStr = Preferences.Get(LastCleanupDateKey, null);
            DateTime lastCleanupDate = lastCleanupStr != null
                ? DateTime.Parse(lastCleanupStr)
                : today.AddDays(-1);

            int daysToRemove = 5;
            int retentionMonths = Preferences.Get(Constants.Constants.Setting.DataRemovalPeriod, 6);
            DateTime retentionCutoffDate = today.AddMonths(-retentionMonths);

            // If lastCleanupDate is already within retention window, no need to clean
            if (lastCleanupDate >= retentionCutoffDate)
                return;

            // Determine how far we can clean this time
            DateTime targetCleanupDate = lastCleanupDate.AddDays(daysToRemove);

            // Ensure we do not delete newer than retentionCutoffDate
            if (targetCleanupDate > retentionCutoffDate)
                targetCleanupDate = retentionCutoffDate;

            // Delete entries between lastCleanupDate and targetCleanupDate (inclusive)
            await DeleteEntriesBetweenAsync(lastCleanupDate, targetCleanupDate);

            // Save the progress
            Preferences.Set(LastCleanupDateKey, targetCleanupDate.ToString("yyyy-MM-dd"));
        }


        private async Task DeleteEntriesBetweenAsync(DateTime lastCleanupDate, DateTime targetCleanupDate)
        {
            var targetsRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var templatesRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var dailyEntriesTask = _dailyEntryRepository.DeleteEntriesBetweenAsync(lastCleanupDate, targetCleanupDate);
            
            var targetsTask = targetsRepo.GetAllAsync();

            var templatesTask = _serviceProvider
                .GetRequiredService<IRepository<FieldTemplate>>()
                .GetListAsync(t => 
                    t.IsDeleted == true);

            await Task.WhenAll(dailyEntriesTask, targetsTask, templatesTask);

            var targets = targetsTask.Result;
            var templates = templatesTask.Result;

            var deletableTemplates = templates.Where(t => !targets.Any(p => p.FieldTemplateId == t.Id));

            var previousMonthDate = lastCleanupDate.AddMonths(-1);
            var deletableTargets = targets
                .Where(t =>
                    t.Month == previousMonthDate.Month &&
                    t.Year == previousMonthDate.Year);
            
            foreach ( var target in deletableTargets )
            {
                await targetsRepo.DeleteAsync(target);
            }

            foreach (var template in deletableTemplates)
            {
                await templatesRepo.DeleteAsync(template);
            }
        }
    }

}
