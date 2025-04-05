using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class FieldTemplate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FieldName { get; set; }
        public string ValueType { get; set; } // "int", "double", "bool" // TODO: remove this.
        public int UnitId { get; set; }
        
        [Ignore]
        public FieldUnit Unit { get; set; }
        
        // for now field name & unit unchangeable
        // we can keep a flag to mark deleted,
        // if user changes anything, new version should be created here (Later)
    }

}
