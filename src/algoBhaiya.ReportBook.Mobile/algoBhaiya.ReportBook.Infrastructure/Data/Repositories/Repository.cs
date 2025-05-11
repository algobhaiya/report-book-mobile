
using algoBhaiya.ReportBooks.Core.Interfaces;
using SQLite;
using System.Linq.Expressions;

namespace algoBhaiya.ReportBook.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : new()
    {
        private readonly SQLiteAsyncConnection _connection;

        public Repository(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _connection.Table<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _connection.Table<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _connection.FindAsync<T>(id);
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _connection.Table<T>().Where(predicate).FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _connection.InsertAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await _connection.UpdateAsync(entity);
        }

        public async Task UpdateAsync(IEnumerable<T> entries)
        {
            await _connection.UpdateAllAsync(entries, true);
        }

        public async Task InsertOrReplaceAsync(T entity)
        {
            await _connection.InsertOrReplaceAsync(entity);
        }

        public async Task InsertAllAsync(IEnumerable<T> entries, bool runInTransaction = true)
        {
            await _connection.InsertAllAsync(entries, runInTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await _connection.DeleteAsync(entity);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity != null)
            {
                await _connection.DeleteAsync(entity);
            }
        }
    }

}
