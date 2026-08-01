namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface ITrackingStreakService
    {
        Task<int> GetCurrentStreakAsync();
        Task<int> GetCurrentStreakAsync(byte userId);
        Task RefreshForCurrentDayAsync(byte userId);
        Task RebuildAsync(byte userId, bool invalidateOnly = false);
        Task NotifyDailyEntryChangedAsync(byte userId, DateTime changedDate);
        Task InvalidateAsync(byte userId);
    }
}
