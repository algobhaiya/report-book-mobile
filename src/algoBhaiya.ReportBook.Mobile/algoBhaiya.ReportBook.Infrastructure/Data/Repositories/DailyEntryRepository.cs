using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using SQLite;

namespace algoBhaiya.ReportBook.Infrastructure.Data.Repositories
{
    public class DailyEntryRepository : Repository<DailyEntry>, IDailyEntryRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public DailyEntryRepository(SQLiteAsyncConnection connection) : base(connection)
        {
            _database = connection;
            _database.CreateTableAsync<DailyEntry>().Wait();
        }

        public Task SaveDailyEntryAsync(DailyEntry entry)
        {
            return _database.InsertOrReplaceAsync(entry);
        }

        public Task SaveDailyEntryAsync(DailyProductivityEntry entry)
        {
            return _database.InsertOrReplaceAsync(entry);
        }

        public Task<List<DailyEntry>> GetEntriesForUserAndDateAsync(int userId, DateTime date) =>
            _database.Table<DailyEntry>()
            .Where(e => e.UserId == userId && e.Date.Date == date.Date)
            .ToListAsync();


        public Task<DailyEntry> GetEntryByDateAsync(DateTime date)
        {
            return _database.Table<DailyEntry>()
                             .Where(x => x.Date == date)
                             .FirstOrDefaultAsync();
        }

        public Task<List<DailyEntry>> GetMonthlyEntriesAsync(int month, int year)
        {
            return _database.Table<DailyEntry>()
                             .Where(x => x.Date.Month == month && x.Date.Year == year)
                             .ToListAsync();
        }
    }
}
