﻿using System.Linq.Expressions;

namespace algoBhaiya.ReportBooks.Core.Interfaces
{
    public interface IRepository<T> where T : new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(int id);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task UpdateAsync(IEnumerable<T> entries);
        Task InsertOrReplaceAsync(T entity);
        Task DeleteAsync(int id);
    }

}
