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
                if (IsInvalidToSave(entry))
                {
                    return;
                }
                await _database.InsertAsync(entry);
            }
            else
            {
                if (IsInvalidToSave(entry))
                {
                    await _database.DeleteAsync(entry);
                }
                else
                {
                    await _database.UpdateAsync(entry);
                }              
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

        public async Task<List<DailySummaryItem>> GetMonthlyEntrySummaryAsync(byte userId, int year, int month)
        {
            var result = new List<DailySummaryItem>();
            try
            {
                // Check validation
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                if (IsInvalidMonth(startDate, DateTime.Today))
                {
                    return result;
                }

                var plans = await _database
                    .Table<MonthlyTarget>()
                    .Where(p => 
                        p.UserId == userId &&
                        p.Month == month &&
                        p.Year == year)
                    .ToListAsync();

                var activePlanCount = plans.Count(p => !p.IsDeleted);

                var deletedPlanFieldIds = plans
                    .Where(p => p.IsDeleted)
                    .Select(p => p.FieldTemplateId)
                    .ToHashSet(); // Optimized for fast lookup

                var entries = await _database.Table<DailyEntry>()
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date < endDate)
                    .ToListAsync();

                var entriesByDate = entries
                    .GroupBy(e => e.Date.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var daysInMonth = DateTime.DaysInMonth(year, month);

                var today = DateTime.Today;
                if (today.Year == year && today.Month == month)
                {
                    daysInMonth = Math.Min(today.Day, daysInMonth);
                }

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    entriesByDate.TryGetValue(date, out var dayEntries);
                    dayEntries ??= new List<DailyEntry>();

                    var filledCount = dayEntries.Count;
                    var deletedCount = dayEntries.Count(e => deletedPlanFieldIds.Contains(e.FieldTemplateId));

                    result.Add(new DailySummaryItem
                    {
                        Date = date,
                        FilledCount = filledCount,
                        TotalFields = activePlanCount + deletedCount
                    });
                }
            }
            catch (Exception ex)
            {
                // handle or log error
            }
            return result;
        }


        #region Private methods
        private bool IsInvalidToSave(DailyEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Value) || entry.Value == "0" || entry.Value == "False")
            {
                return true;
            }

            return false;
        }

        private bool IsInvalidMonth(DateTime startDate, DateTime currentDateTime)
        {
            if (((startDate.Month > currentDateTime.Month) 
                    && (startDate.Year == currentDateTime.Year))
                || startDate.Year > currentDateTime.Year)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
