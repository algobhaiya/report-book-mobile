namespace algoBhaiya.ReportBooks.Core.Interfaces
{
    public interface IRepository<T> where T : new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

}
