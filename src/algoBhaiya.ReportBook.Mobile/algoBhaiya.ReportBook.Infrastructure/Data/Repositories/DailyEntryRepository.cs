using algoBhaiya.ReportBook.Core.Dtos;
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

        public async Task<List<DailySummaryItem>> GetMonthlyEntrySummaryAsync(int userId, int year, int month)
        {
            var result = new List<DailySummaryItem>();
            try
            {
                var totalFields = await _database.Table<FieldTemplate>().CountAsync();

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                var entries = await _database.Table<DailyEntry>()
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date < endDate)
                    .ToListAsync();

                var entriesByDate = entries
                    .GroupBy(e => e.Date.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var daysInMonth = DateTime.DaysInMonth(year, month);

                var currentDateTime = DateTime.Today;
                if (currentDateTime.Year == year && currentDateTime.Month == month)
                {
                    daysInMonth = Math.Min(currentDateTime.Day, daysInMonth);
                }

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var filled = entriesByDate.ContainsKey(date) ? entriesByDate[date].Count : 0;

                    result.Add(new DailySummaryItem
                    {
                        Date = date,
                        FilledCount = filled,
                        TotalFields = totalFields
                    });
                }
            }
            catch (Exception ex)
            {
                // handle or log error
            }
            return result;
        }

    }
}
