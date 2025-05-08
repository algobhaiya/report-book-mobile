using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    public class DataRetentionService : IDataRetentionService
    {
        private readonly string LastCleanupDateKey;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDailyEntryRepository _dailyEntryRepository;

        public DataRetentionService(
            IServiceProvider serviceProvider,
            IDailyEntryRepository dailyEntryRepository)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException();
            _dailyEntryRepository = dailyEntryRepository ?? throw new ArgumentNullException();
            LastCleanupDateKey = Constants.Constants.AppState.LastCleanupDateKey;
        }
        
        public async Task PerformIncrementalCleanupAsync()
        {           
            int daysToRemove = 5;
            int retentionMonths = Preferences.Get(Constants.Constants.Setting.DataRemovalPeriod, 6);

            DateTime today = DateTime.UtcNow.Date;
            DateTime retentionCutoffDate = today.AddMonths(-retentionMonths);

            string? lastCleanupStr = Preferences.Get(LastCleanupDateKey, null);

            // First-time run: initialize cleanup marker but skip cleanup
            if (string.IsNullOrEmpty(lastCleanupStr) ||
                !DateTime.TryParse(lastCleanupStr, out DateTime lastCleanupDate))
            {
                Preferences.Set(LastCleanupDateKey, retentionCutoffDate.ToString("yyyy-MM-dd"));
                return;
            }

            // If lastCleanupDate is already within retention window, no need to clean
            if (lastCleanupDate >= retentionCutoffDate)
                return;

            // Determine how far we can clean this time
            DateTime targetCleanupDate = lastCleanupDate.AddDays(daysToRemove);

            // Ensure we do not delete newer than retentionCutoffDate
            if (targetCleanupDate > retentionCutoffDate)
                targetCleanupDate = retentionCutoffDate;

            // Delete entries between lastCleanupDate and targetCleanupDate (inclusive)
            await DeleteEntriesBetweenAsync(targetCleanupDate);

            // Save the progress
            Preferences.Set(LastCleanupDateKey, targetCleanupDate.ToString("yyyy-MM-dd"));
        }


        private async Task DeleteEntriesBetweenAsync(DateTime targetCleanupDate)
        {
            var targetsRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var templatesRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var dailyEntriesTask = _dailyEntryRepository.DeleteEntriesBetweenAsync(targetCleanupDate);
            
            var targetsTask = targetsRepo.GetAllAsync();

            var templatesTask = templatesRepo
                .GetListAsync(t => 
                    t.IsDeleted == true);

            await Task.WhenAll(dailyEntriesTask, targetsTask, templatesTask);

            var allTargets = targetsTask.Result;
            var deletedTemplates = templatesTask.Result;

            var targetTemplateIds = new HashSet<int>(allTargets.Select(t => t.FieldTemplateId));
            var deletableTemplates = deletedTemplates
                .Where(t => !targetTemplateIds.Contains(t.Id))
                .ToList();

            var previousMonthDate = targetCleanupDate.AddMonths(-1);
            var deletableTargets = allTargets
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
