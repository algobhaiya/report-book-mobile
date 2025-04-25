using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class FieldTemplate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public byte UserId { get; set; }
        public string FieldName { get; set; }
        public string ValueType { get; set; } // "int", "double", "bool" // TODO: remove this.
        public byte UnitId { get; set; }
        public byte FieldOrder { get; set; }
        public bool IsEnabled { get; set; } = true;

        [Ignore]
        public FieldUnit Unit { get; set; }
        
        // for now field name & unit unchangeable
        // we can keep a flag to mark deleted,
        // if user changes anything, new version should be created here (Later)
    }

}
