
namespace algoBhaiya.ReportBook.Core.Entities
{
    public class MonthlySummaryItem
    {
        public string ItemName { get; set; }
        public string TotalDays { get; set; }
        public string AverageValue { get; set; }
        public string TotalSum { get; set; }
        public string Target { get; set; }
        public List<DateTime> FilledDates { get; set; }
    }
}
