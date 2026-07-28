using algoBhaiya.ReportBook.Core.Entities;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class PlannerPresetGroup : ObservableCollection<PlannerPreset>
    {
        public string Title { get; }

        public PlannerPresetGroup(string title, IEnumerable<PlannerPreset> items)
            : base(items)
        {
            Title = title;
        }
    }
}
