﻿
using SQLite;

namespace algoBhaiya.ReportBook.Core.Entities
{
    public class AppUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UserName { get; set; }
        public string PasswordHash { get; set; } // Simplified login for local
    }

}
