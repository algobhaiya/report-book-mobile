using algoBhaiya.ReportBook.Core.Entities;
using SQLite;

namespace algoBhaiya.ReportBook.Infrastructure.Data
{
    public class DatabaseInitializer
    {
        private readonly SQLiteAsyncConnection _connection;

        public DatabaseInitializer(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        public async Task InitializeAsync()
        {
            await _connection.CreateTableAsync<AppUser>();
            await _connection.CreateTableAsync<FieldUnit>();
            await _connection.CreateTableAsync<FieldTemplate>();
            await _connection.CreateTableAsync<MonthlyTarget>();
            await _connection.CreateTableAsync<DailyEntry>();
        }
    }

}
