
namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntrySummaryViewModel
    {
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public int FilledCount { get; set; }
        public int TotalCount { get; set; }
        public string StatusText { get; set; }
        public string StatusIcon { get; set; }
    }

}
