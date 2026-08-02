using algoBhaiya.ReportBook.Core.Dtos;
using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IDailyEntryRepository
    {
        Task SaveDailyEntryAsync(DailyEntry entry);        
        Task<List<DailyEntry>> GetEntriesForUserAndDateAsync(int userId, DateTime date);
        Task<bool> HasEntriesForUserAndDateAsync(int userId, DateTime date);
        Task<List<DailyEntry>> GetEntriesForUserThroughDateAsync(int userId, DateTime toDate);
        Task<DateTime?> GetLatestTrackedDateForUserAsync(int userId, DateTime toDate);
        Task<List<DateTime>> GetTrackedDatesForUserThroughDateAsync(int userId, DateTime toDate);
        Task<DailyEntry> GetEntryByDateAsync(DateTime date);
        Task<List<DailyEntry>> GetMonthlyEntriesAsync(int month, int year);
        Task<List<DailySummaryItem>> GetMonthlyEntrySummaryAsync(byte userId, int year, int month);
        Task<List<MonthlySummaryItem>> GetMonthlySummaryReportAsync(byte userId, int year, int month);
        Task<int> DeleteEntriesBetweenAsync(DateTime toDate);
    }
}
