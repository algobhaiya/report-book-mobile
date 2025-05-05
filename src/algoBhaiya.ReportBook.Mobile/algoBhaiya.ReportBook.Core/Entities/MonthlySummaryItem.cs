
namespace algoBhaiya.ReportBook.Core.Entities
{
    public class MonthlySummaryItem
    {
        public string ItemName { get; set; }
        public int TotalDays { get; set; }
        public double AverageValue { get; set; }
        public double TotalSum { get; set; }
        public double Target { get; set; }
        public List<DateTime> FilledDates { get; set; }
    }
}
