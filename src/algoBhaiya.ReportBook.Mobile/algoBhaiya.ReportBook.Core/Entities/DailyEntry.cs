﻿using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class DailyEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public byte UserId { get; set; }
        public int FieldTemplateId { get; set; }        

        public DateTime Date { get; set; }

        public string Value { get; set; } // Store all as string and cast based on ValueType

        [Ignore]
        public FieldTemplate FieldTemplate { get; set; }
    }

}
