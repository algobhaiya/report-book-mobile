using algoBhaiya.ReportBook.Core.Dtos;
using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IDailyEntryRepository
    {
        Task SaveDailyEntryAsync(DailyEntry entry);        
        Task<List<DailyEntry>> GetEntriesForUserAndDateAsync(int userId, DateTime date);
        Task<DailyEntry> GetEntryByDateAsync(DateTime date);
        Task<List<DailyEntry>> GetMonthlyEntriesAsync(int month, int year);
        Task<List<DailySummaryItem>> GetMonthlyEntrySummaryAsync(byte userId, int year, int month);
        Task<List<MonthlySummaryItem>> GetMonthlySummaryReportAsync(byte userId, int year, int month);
    }
}
