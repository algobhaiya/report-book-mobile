﻿using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class FieldUnit
    {
        [PrimaryKey, AutoIncrement]
        public byte Id { get; set; }

        public string UnitName { get; set; }  // "Hours", "Minutes", "Checkbox"
        public string ValueType { get; set; } // "int", "double", "bool" // Should match FieldTemplate.ValueType
        
        public bool IsDeleted { get; set; } = false;
    }

}
