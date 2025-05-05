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
                var today = DateTime.Today;

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

                // IF plan is empty,
                // THEN set only the current plan.
                // Can not add for previous month.
                // To set future month, User can open the monthly plan.
                // This is only to set for default plan.
                // Normally, (current or future) plan updates onChange event of field template add, edit, delete.
                if (plans.Count == 0 &&
                    today.Month == month &&
                    today.Year == year)
                {
                    var monthlyPlansToAdd = new List<MonthlyTarget>();

                    var activeFields = await _database
                        .Table<FieldTemplate>()
                        .Where(f =>
                            f.UserId == userId &&
                            !f.IsDeleted &&
                            f.IsEnabled)
                        .ToListAsync();

                    foreach (var item in activeFields)
                    {
                        var plan = new MonthlyTarget
                        {
                            UserId = userId,
                            FieldTemplateId = item.Id,
                            Month = (byte) month,
                            Year = year,
                            FieldOrder = item.FieldOrder,
                        };
                        monthlyPlansToAdd.Add(plan);
                    }
                    await _database.InsertAllAsync(monthlyPlansToAdd, true);

                    plans = monthlyPlansToAdd;
                }

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

        public async Task<List<MonthlySummaryItem>> GetMonthlySummaryReportAsync(byte userId, int year, int month)
        {
            var result = new List<MonthlySummaryItem>();
            try
            {
                var today = DateTime.Today;

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
                    .OrderBy(p => p.IsDeleted)
                    .ThenBy(p => p.FieldOrder)
                    .ToListAsync();

                // IF plan is empty,
                // THEN set only the current plan.
                // Can not add for previous month.
                // To set future month, User can open the monthly plan.
                // This is only to set for default plan.
                // Normally, (current or future) plan updates onChange event of field template add, edit, delete.
                if (plans.Count == 0 &&
                    today.Month == month &&
                    today.Year == year)
                {
                    var monthlyPlansToAdd = new List<MonthlyTarget>();

                    var activeFields = await _database
                        .Table<FieldTemplate>()
                        .Where(f =>
                            f.UserId == userId &&
                            !f.IsDeleted &&
                            f.IsEnabled)
                        .ToListAsync();

                    foreach (var item in activeFields)
                    {
                        var plan = new MonthlyTarget
                        {
                            UserId = userId,
                            FieldTemplateId = item.Id,
                            Month = (byte)month,
                            Year = year,
                            FieldOrder = item.FieldOrder,
                        };
                        monthlyPlansToAdd.Add(plan);
                    }
                    await _database.InsertAllAsync(monthlyPlansToAdd, true);

                    plans = monthlyPlansToAdd;
                }

                var templatesTask = _database
                    .Table<FieldTemplate>()
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                var unitsTask = _database.Table<FieldUnit>().ToListAsync();

                var entriesTask = _database.Table<DailyEntry>()
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date < endDate)
                    .ToListAsync();

                await Task.WhenAll(templatesTask, unitsTask, entriesTask);

                var fieldTemplates = templatesTask.Result;
                var units = unitsTask.Result;
                var entries = entriesTask.Result;

                var fieldTemplateLookup = fieldTemplates
                    .Where(t => plans.Any(p => p.FieldTemplateId == t.Id))
                    .ToDictionary(t => t.Id);

                var unitLookup = units.ToDictionary(u => u.Id);

                var entriesByItem = entries
                    .GroupBy(e => e.FieldTemplateId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var daysInMonth = DateTime.DaysInMonth(year, month);

                foreach (var planItem in plans)
                {
                    entriesByItem.TryGetValue(planItem.FieldTemplateId, out var ItemSummaries);
                    ItemSummaries ??= new List<DailyEntry>();

                    // Skip deleted field, if it has no daily report entries.
                    if (planItem.IsDeleted && entriesByItem.Count == 0)
                    {
                        continue;
                    }

                    if (!fieldTemplateLookup.TryGetValue(planItem.FieldTemplateId, out var template))
                        continue;

                    if (!unitLookup.TryGetValue(template.UnitId, out var unit))
                        continue;

                    template.Unit = unit;

                    Type unitType = GetUnitValueType(unit.ValueType);

                    double sum = 0;

                    foreach (var entry in ItemSummaries)
                    {
                        sum += GetConvertedValue(unitType, entry.Value);
                    }

                    result.Add(new MonthlySummaryItem
                    {
                        ItemName = template.FieldName,
                        TotalDays = ItemSummaries.Count.ToString(),
                        AverageValue = Math.Round(sum / daysInMonth, 2).ToString(),
                        TotalSum = sum.ToString(),
                        Target = planItem.TargetValue,
                        FilledDates = ItemSummaries.Select(x => x.Date).ToList() ?? new List<DateTime>()
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

        private Type GetUnitValueType(string unitType)
        {
            switch (unitType)
            {
                case "int": return typeof(int);
                case "double": return typeof(double);
                case "bool": return typeof(bool);
            }

            return typeof(string); // default
        }

        private double GetConvertedValue(Type valueType, string value)
        {
            double convertedValue = 0;

            if (valueType == typeof(int) && int.TryParse(value, out var intVal))
                convertedValue += intVal;
            else if (valueType == typeof(double) && double.TryParse(value, out var dblVal))
                convertedValue += dblVal;
            else if (valueType == typeof(bool) && bool.TryParse(value, out var boolVal))
                convertedValue += boolVal ? 1 : 0;

            return convertedValue;
        }

        #endregion
    }
}
