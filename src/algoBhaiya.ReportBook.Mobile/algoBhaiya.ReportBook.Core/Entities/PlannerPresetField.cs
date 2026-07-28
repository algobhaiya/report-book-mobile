using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class PlannerPresetField
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PlannerPresetId { get; set; }
        public string FieldName { get; set; }
        public byte UnitId { get; set; }
        public string ValueType { get; set; }
        public byte FieldOrder { get; set; }

        [Ignore]
        public FieldUnit Unit { get; set; }
    }
}
