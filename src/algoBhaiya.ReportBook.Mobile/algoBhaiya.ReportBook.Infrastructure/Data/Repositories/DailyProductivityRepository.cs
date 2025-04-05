using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using SQLite;

namespace algoBhaiya.ReportBook.Infrastructure.Data.Repositories
{
    public class DailyProductivityRepository : Repository<DailyProductivityEntry>, IDailyProductivityRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public DailyProductivityRepository(SQLiteAsyncConnection connection) : base(connection)
        {
            _database = connection;
            _database.CreateTableAsync<DailyProductivityEntry>().Wait();
        }

        public Task SaveDailyEntryAsync(DailyProductivityEntry entry)
        {
            return _database.InsertOrReplaceAsync(entry);
        }

        public Task<DailyProductivityEntry> GetEntryByDateAsync(DateTime date)
        {
            return _database.Table<DailyProductivityEntry>()
                             .Where(x => x.Date == date)
                             .FirstOrDefaultAsync();
        }

        public Task<List<DailyProductivityEntry>> GetMonthlyEntriesAsync(int month, int year)
        {
            return _database.Table<DailyProductivityEntry>()
                             .Where(x => x.Date.Month == month && x.Date.Year == year)
                             .ToListAsync();
        }
    }
}
