using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using SQLite;

namespace algoBhaiya.ReportBook.Infrastructure.Data.Repositories
{
    public class MonthlyTargetRepository : Repository<MonthlyTarget>, IMonthlyTargetRepository
    {
        private readonly SQLiteAsyncConnection _db;
        private static readonly SemaphoreSlim _saveGate = new(1, 1);

        public MonthlyTargetRepository(SQLiteAsyncConnection connection) : base(connection)
        {
            _db = connection;
        }

        public Task<List<MonthlyTarget>> GetMonthlyTargetsAsync(int userId, int year, int month) =>
            _db.Table<MonthlyTarget>()
               .Where(t => t.UserId == userId && t.Month == month && t.Year == year)
               .ToListAsync();

        public async Task SaveMonthlyTargetAsync(MonthlyTarget target)
        {
            await _saveGate.WaitAsync();
            try
            {
                var existing = await _db.Table<MonthlyTarget>()
                    .Where(t => t.UserId == target.UserId &&
                                t.FieldTemplateId == target.FieldTemplateId &&
                                t.Month == target.Month &&
                                t.Year == target.Year)
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.TargetValue = target.TargetValue;
                    existing.FieldOrder = target.FieldOrder;
                    existing.IsDeleted = target.IsDeleted;
                    await _db.UpdateAsync(existing);
                    return;
                }

                await _db.InsertAsync(target);
            }
            finally
            {
                _saveGate.Release();
            }
        }

    }
}
