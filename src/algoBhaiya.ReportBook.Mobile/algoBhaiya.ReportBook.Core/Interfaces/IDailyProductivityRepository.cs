using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IDailyProductivityRepository
    {
        Task SaveDailyEntryAsync(DailyProductivityEntry entry);
        Task<DailyProductivityEntry> GetEntryByDateAsync(DateTime date);
        Task<List<DailyProductivityEntry>> GetMonthlyEntriesAsync(int month, int year);
    }
}
