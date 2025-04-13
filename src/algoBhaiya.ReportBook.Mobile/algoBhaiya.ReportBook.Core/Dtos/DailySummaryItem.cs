
namespace algoBhaiya.ReportBook.Core.Dtos
{
    public class DailySummaryItem
    {
        public DateTime Date { get; set; }
        public int FilledCount { get; set; }
        public int TotalFields { get; set; }
        public bool IsCompleted => FilledCount == TotalFields;
    }

}
