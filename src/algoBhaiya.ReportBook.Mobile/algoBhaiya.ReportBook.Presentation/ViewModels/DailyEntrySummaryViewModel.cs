namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public enum DailyEntryStatusState
    {
        Pending,
        Incomplete,
        Completed
    }

    public class DailyEntrySummaryViewModel
    {
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public int FilledCount { get; set; }
        public int TotalCount { get; set; }
        public string StatusText { get; set; }
        public string StatusIcon { get; set; }
        public string StatusSupportText { get; set; }
        public DailyEntryStatusState StatusState =>
            FilledCount <= 0
                ? DailyEntryStatusState.Pending
                : FilledCount >= TotalCount
                    ? DailyEntryStatusState.Completed
                    : DailyEntryStatusState.Incomplete;

        public string StatusBadgeText => StatusState switch
        {
            DailyEntryStatusState.Completed => "Done",
            DailyEntryStatusState.Incomplete => "Partial",
            _ => "Pending"
        };

        public string StatusBadgeSubtitle => StatusState switch
        {
            DailyEntryStatusState.Completed => "Great job",
            DailyEntryStatusState.Incomplete => "Keep going",
            _ => "Start now"
        };

        public Color StatusBadgeBackgroundColor => StatusState switch
        {
            DailyEntryStatusState.Completed => Color.FromArgb("#E8F5E9"),
            DailyEntryStatusState.Incomplete => Color.FromArgb("#EEF6FF"),
            _ => Color.FromArgb("#FFF8E1")
        };

        public Color StatusBadgeBorderColor => StatusState switch
        {
            DailyEntryStatusState.Completed => Color.FromArgb("#43A047"),
            DailyEntryStatusState.Incomplete => Color.FromArgb("#8CBFEA"),
            _ => Color.FromArgb("#F9A825")
        };

        public Color StatusBadgeTextColor => StatusState switch
        {
            DailyEntryStatusState.Completed => Color.FromArgb("#1B5E20"),
            DailyEntryStatusState.Incomplete => Color.FromArgb("#2A6DB0"),
            _ => Color.FromArgb("#7A5A00")
        };

    }

}
