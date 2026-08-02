using System.Text.Json;
using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    public class TrackingStreakService : ITrackingStreakService
    {
        private const string CacheVersion = "1";
        private const string CacheKeyPrefix = "TrackingStreakCache";
        private const string CacheDayKeyPrefix = "TrackingStreakCacheDay";

        private readonly IDailyEntryRepository _dailyEntryRepository;

        public TrackingStreakService(IDailyEntryRepository dailyEntryRepository)
        {
            _dailyEntryRepository = dailyEntryRepository;
        }

        public Task<int> GetCurrentStreakAsync()
        {
            byte userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            return GetCurrentStreakAsync(userId);
        }

        public async Task<int> GetCurrentStreakAsync(byte userId)
        {
            if (userId == 0)
            {
                return 0;
            }

            await RefreshForCurrentDayAsync(userId);
            var cache = await GetOrBuildCacheAsync(userId);
            return GetLength(cache);
        }

        public async Task RefreshForCurrentDayAsync(byte userId)
        {
            if (userId == 0)
            {
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var lastChecked = LoadLastCheckedDate(userId);
            if (lastChecked.HasValue && lastChecked.Value == today)
            {
                return;
            }

            var cache = LoadCache(userId);
            if (!cache.StartDate.HasValue || !cache.EndDate.HasValue)
            {
                SaveLastCheckedDate(userId, today);
                return;
            }

            var currentEnd = cache.EndDate.Value.ToDateTime(TimeOnly.MinValue);
            var currentStart = cache.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            var currentDay = today.ToDateTime(TimeOnly.MinValue);
            var yesterday = currentDay.AddDays(-1);

            if (currentEnd < yesterday)
            {
                SaveLastCheckedDate(userId, today);
                return;
            }

            if (await IsDateTrackedAsync(userId, currentDay))
            {
                cache.EndDate = today;
                SaveCache(userId, cache);
            }
            else if (currentEnd >= currentDay)
            {
                var newEndDate = yesterday;
                if (newEndDate < currentStart)
                {
                    ClearCache(userId);
                }
                else
                {
                    cache.EndDate = DateOnly.FromDateTime(newEndDate);
                    SaveCache(userId, cache);
                }
            }
            SaveLastCheckedDate(userId, today);
        }

        public async Task RebuildAsync(byte userId, bool invalidateOnly = false)
        {
            if (userId == 0)
            {
                return;
            }

            if (invalidateOnly)
            {
                ClearCache(userId);
                return;
            }

            var cache = await BuildCacheAsync(userId);
            SaveCache(userId, cache);
        }

        public async Task NotifyDailyEntryChangedAsync(byte userId, DateTime changedDate)
        {
            if (userId == 0)
            {
                return;
            }

            var today = DateTime.Today.Date;
            var changed = changedDate.Date;
            if (changed > today)
            {
                return;
            }

            var cache = await GetOrBuildCacheAsync(userId);
            var isTracked = await IsDateTrackedAsync(userId, changed);

            if (!cache.StartDate.HasValue || !cache.EndDate.HasValue)
            {
                if (isTracked)
                {
                    await RebuildAsync(userId);
                }
                return;
            }

            var startDate = cache.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            var endDate = cache.EndDate.Value.ToDateTime(TimeOnly.MinValue);

            if (changed == today)
            {
                if (isTracked)
                {
                    if (cache.EndDate.Value != DateOnly.FromDateTime(today))
                    {
                        cache.EndDate = DateOnly.FromDateTime(today);
                        SaveCache(userId, cache);
                    }
                }
                else
                {
                    var newEndDate = today.AddDays(-1);
                    if (newEndDate < startDate)
                    {
                        ClearCache(userId);
                        return;
                    }

                    cache.EndDate = DateOnly.FromDateTime(newEndDate);
                    SaveCache(userId, cache);
                }

                return;
            }

            if (isTracked)
            {                
                if (changed == startDate.AddDays(-1))
                {
                    await RebuildAsync(userId);
                    return;
                }
               
                return;
            }

            if (changed < startDate)
            {
                return;
            }

            if (changed > endDate)
            {
                return;
            }

            if (changed == endDate)
            {
                ClearCache(userId);
                return;
            }

            var shrunkStartDate = changed.AddDays(1);
            if (shrunkStartDate > endDate)
            {
                ClearCache(userId);
                return;
            }

            cache.StartDate = DateOnly.FromDateTime(shrunkStartDate);
            SaveCache(userId, cache);
        }

        public Task InvalidateAsync(byte userId)
        {
            ClearCache(userId);
            return Task.CompletedTask;
        }

        private async Task<StreakCache> GetOrBuildCacheAsync(byte userId)
        {
            var cache = LoadCache(userId);
            if (cache.StartDate.HasValue && cache.EndDate.HasValue)
            {
                return cache;
            }

            cache = await BuildCacheAsync(userId);
            SaveCache(userId, cache);
            return cache;
        }

        private async Task<StreakCache> BuildCacheAsync(byte userId)
        {
            var today = DateTime.Today.Date;
            var latestTrackedDate = await _dailyEntryRepository.GetLatestTrackedDateForUserAsync(userId, today);
            if (!latestTrackedDate.HasValue)
            {
                return new StreakCache();
            }

            var anchorDate = latestTrackedDate.Value;
            if (anchorDate != today && anchorDate != today.AddDays(-1))
            {
                return new StreakCache();
            }

            var trackedDates = await _dailyEntryRepository.GetTrackedDatesForUserThroughDateAsync(userId, anchorDate);
            var trackedDateSet = trackedDates.ToHashSet();

            var cursor = anchorDate;
            while (trackedDateSet.Contains(cursor.AddDays(-1)))
            {
                cursor = cursor.AddDays(-1);
            }

            return new StreakCache
            {
                StartDate = DateOnly.FromDateTime(cursor),
                EndDate = DateOnly.FromDateTime(anchorDate)
            };
        }

        private async Task<bool> IsDateTrackedAsync(byte userId, DateTime date)
        {
            if (date.Date > DateTime.Today.Date)
            {
                return false;
            }

            return await _dailyEntryRepository.HasEntriesForUserAndDateAsync(userId, date.Date);
        }

        private static int GetLength(StreakCache cache)
        {
            if (!cache.StartDate.HasValue || !cache.EndDate.HasValue)
            {
                return 0;
            }

            return cache.EndDate.Value.DayNumber - cache.StartDate.Value.DayNumber + 1;
        }

        private static string GetCacheKey(byte userId) => $"{CacheKeyPrefix}:{CacheVersion}:{userId}";
        private static string GetLastCheckedKey(byte userId) => $"{CacheDayKeyPrefix}:{CacheVersion}:{userId}";

        private static StreakCache LoadCache(byte userId)
        {
            var json = Preferences.Get(GetCacheKey(userId), string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new StreakCache();
            }

            try
            {
                return JsonSerializer.Deserialize<StreakCache>(json) ?? new StreakCache();
            }
            catch
            {
                return new StreakCache();
            }
        }

        private static void SaveCache(byte userId, StreakCache cache)
        {
            Preferences.Set(GetCacheKey(userId), JsonSerializer.Serialize(cache));
        }

        private static void ClearCache(byte userId)
        {
            Preferences.Remove(GetCacheKey(userId));
        }

        private static DateOnly? LoadLastCheckedDate(byte userId)
        {
            var text = Preferences.Get(GetLastCheckedKey(userId), string.Empty);
            return DateOnly.TryParse(text, out var date) ? date : null;
        }

        private static void SaveLastCheckedDate(byte userId, DateOnly date)
        {
            Preferences.Set(GetLastCheckedKey(userId), date.ToString("yyyy-MM-dd"));
        }
    }
}
