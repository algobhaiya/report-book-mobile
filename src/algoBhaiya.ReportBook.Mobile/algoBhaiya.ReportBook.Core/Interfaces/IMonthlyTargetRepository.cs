using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IMonthlyTargetRepository
    {
        Task<List<MonthlyTarget>> GetMonthlyTargetsAsync(int userId, int month, int year);
        Task SaveMonthlyTargetAsync(MonthlyTarget target);
    }
}
