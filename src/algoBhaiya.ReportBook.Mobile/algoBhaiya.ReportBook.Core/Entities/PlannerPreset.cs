using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class PlannerPreset
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string AccentHex { get; set; }
        public byte SortOrder { get; set; }
        public bool IsBlank { get; set; }

        [Ignore]
        public IReadOnlyList<PlannerPresetField> Fields { get; init; } = Array.Empty<PlannerPresetField>();

        [Ignore]
        public string FieldCountText => $"{Fields.Count} fields";
    }
}
