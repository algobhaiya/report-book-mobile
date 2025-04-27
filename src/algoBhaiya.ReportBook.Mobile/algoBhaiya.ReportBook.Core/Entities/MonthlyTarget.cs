using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class MonthlyTarget
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public byte UserId { get; set; }
        public int FieldTemplateId { get; set; }

        public byte Month { get; set; }
        public int Year { get; set; }

        public string TargetValue { get; set; }
        public byte FieldOrder { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
    }

}
