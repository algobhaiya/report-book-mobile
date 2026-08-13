namespace algoBhaiya.ReportBook.Core.Dtos
{
    public class StreakRefreshResult
    {
        public int StreakCount { get; set; }

        public bool HadPositiveStreakBeforeRefresh { get; set; }

        public bool IsStartupLoss { get; set; }
    }
}
