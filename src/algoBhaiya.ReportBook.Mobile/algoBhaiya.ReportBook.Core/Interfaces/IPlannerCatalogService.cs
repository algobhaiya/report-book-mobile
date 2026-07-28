using algoBhaiya.ReportBook.Core.Entities;

namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IPlannerCatalogService
    {
        Task<IReadOnlyList<PlannerPreset>> GetCatalogAsync();
        Task<bool> HasActiveFieldsAsync(byte userId);
        Task StartPresetAsync(byte userId, PlannerPreset preset);
    }
}
