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

        public async Task SaveDailyEntryAsync(DailyEntry entry)
        {
            if (entry.Id == 0)
            {
                await _database.InsertAsync(entry);
            }
            else
            {
                await _database.UpdateAsync(entry);
            }            
        }

        public Task SaveDailyEntryAsync(DailyProductivityEntry entry)
        {
            return _database.InsertOrReplaceAsync(entry);
        }
        
        public async Task<List<DailyEntry>> GetEntriesForUserAndDateAsync(int userId, DateTime date)
        {
            var list = new List<DailyEntry>();
            try
            {
                var start = date.Date;
                var end = start.AddDays(1);

                list = await _database.Table<DailyEntry>()
                    .Where(e => e.UserId == userId && e.Date >= start && e.Date < end)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                
            }
            return list;
        }

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
