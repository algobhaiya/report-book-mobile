using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IMonthlyTargetRepository
    {
        Task<List<MonthlyTarget>> GetMonthlyTargetsAsync(int userId, int year, int month);
        Task SaveMonthlyTargetAsync(MonthlyTarget target);
    }
}
