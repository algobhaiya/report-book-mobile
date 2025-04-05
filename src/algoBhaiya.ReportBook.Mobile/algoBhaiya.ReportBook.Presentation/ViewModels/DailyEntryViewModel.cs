using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryViewModel : INotifyPropertyChanged
    {
        private readonly IDailyEntryRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<DailyEntryFieldViewModel> Fields { get; set; } = new();

        public DailyEntryViewModel(
            IServiceProvider serviceProvider,
            IDailyEntryRepository repository)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;
            LoadDataCommand = new Command(async () => await LoadFormFieldsAsync());
            SaveCommand = new Command(async () => await SaveFormAsync());
        }

        public Command LoadDataCommand { get; }
        public Command SaveCommand { get; }

        public async Task LoadFormFieldsAsync()
        {
            Fields.Clear();
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) return;

            var fieldTemplateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();
            var fieldUnitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

            var templates = await fieldTemplateRepo.GetAllAsync();
            var units = await fieldUnitRepo.GetAllAsync();
            var entries = await _repository.GetEntriesForUserAndDateAsync(userId, DateTime.Today);

            foreach (var template in templates)
            {
                var unit = units.FirstOrDefault(u => u.Id == template.UnitId);
                var entry = entries.FirstOrDefault(e => e.FieldTemplateId == template.Id);

                Fields.Add(new DailyEntryFieldViewModel
                {
                    FieldTemplateId = template.Id,
                    FieldName = template.FieldName,
                    ValueType = template.ValueType,
                    UnitName = unit?.UnitName ?? "",
                    UserId = userId,
                    Date = DateTime.Today,
                    Value = entry?.Value ?? ""
                });
            }
        }

        public async Task SaveFormAsync()
        {
            foreach (var field in Fields)
            {
                await _repository.SaveDailyEntryAsync(new DailyEntry
                {
                    FieldTemplateId = field.FieldTemplateId,
                    UserId = field.UserId,
                    Date = field.Date,
                    Value = field.Value
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
