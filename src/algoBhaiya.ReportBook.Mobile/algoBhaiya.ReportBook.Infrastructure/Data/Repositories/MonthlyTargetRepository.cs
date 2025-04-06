using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using SQLite;

namespace algoBhaiya.ReportBook.Infrastructure.Data.Repositories
{
    public class MonthlyTargetRepository : Repository<MonthlyTarget>, IMonthlyTargetRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public MonthlyTargetRepository(SQLiteAsyncConnection connection) : base(connection)
        {
            _db = connection;
            _db.CreateTableAsync<MonthlyTarget>().Wait();
        }

        public Task<List<MonthlyTarget>> GetMonthlyTargetsAsync(int userId, int month, int year) =>
            _db.Table<MonthlyTarget>()
               .Where(t => t.UserId == userId && t.Month == month && t.Year == year)
               .ToListAsync();

        public async Task SaveMonthlyTargetAsync(MonthlyTarget target)
        {
            var existing = await _db.Table<MonthlyTarget>()
                .FirstOrDefaultAsync(t => t.UserId == target.UserId && t.FieldTemplateId == target.FieldTemplateId &&
                                          t.Month == target.Month && t.Year == target.Year);

            if (existing != null)
            {
                existing.TargetValue = target.TargetValue;
                await _db.UpdateAsync(existing);
            }
            else
            {
                await _db.InsertAsync(target);
            }
        }

    }
}
