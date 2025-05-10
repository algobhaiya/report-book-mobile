
namespace algoBhaiya.ReportBook.Core.Entities
{
    public class MonthlySummaryItem
    {
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public string TotalDays { get; set; }
        public string AverageValue { get; set; }
        public string TotalSum { get; set; }
        public string Percentage { get; set; }
        public List<DateTime> FilledDates { get; set; }
    }
}
