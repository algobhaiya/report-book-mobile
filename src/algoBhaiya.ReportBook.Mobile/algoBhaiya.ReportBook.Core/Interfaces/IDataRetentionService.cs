namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IDataRetentionService
    {
        Task PerformIncrementalCleanupAsync();
    }

}
