using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class MonthlyTarget
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public byte UserId { get; set; }
        public int FieldTemplateId { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public string TargetValue { get; set; }
    }

}
