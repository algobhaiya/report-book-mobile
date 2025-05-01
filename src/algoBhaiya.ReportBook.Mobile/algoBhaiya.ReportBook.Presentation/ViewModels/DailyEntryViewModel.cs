using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DailyEntry> Fields { get; set; } = new();

        public ICommand SubmitCommand { get; }
        public ICommand LoadCommand { get; }

        public DateTime? LoadingDateTime { get; set; } = null;
        private DateTime _effectiveDate;
        public DateTime FormDate 
        { 
            get => _effectiveDate;
            set
            {
                if (_effectiveDate != value)
                {
                    _effectiveDate = value;
                    OnPropertyChanged();
                }
            }

        }
        
        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanSubmit));
                }
            }
        }
        public bool CanSubmit => !IsReadOnly;


        private readonly IDailyEntryRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public DailyEntryViewModel(
            IServiceProvider serviceProvider,
            IDailyEntryRepository repository
            )
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            SubmitCommand = new Command(async () => await SubmitAsync());
            LoadCommand = new Command(async () => await LoadFieldsAsync());            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async Task LoadFieldsAsync()
        {
            Fields.Clear();

            byte userId = (byte)Preferences.Get("CurrentUserId", 0);
            if (userId == 0)
                return;

            FormDate = LoadingDateTime ?? DateTime.Today;
            IsReadOnly = (DateTime.Today - FormDate).Days > 14;

            var targetRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var templateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();
            var unitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

            var plannedFieldsTask = targetRepo
                .GetListAsync(f =>
                    f.UserId == userId &&
                    f.Month == FormDate.Month &&
                    f.Year == FormDate.Year);

            var templatesTask = templateRepo.GetListAsync(f => f.UserId == userId);
            var unitsTask = unitRepo.GetAllAsync();
            var entriesTask = _repository.GetEntriesForUserAndDateAsync(userId, FormDate);

            await Task.WhenAll(plannedFieldsTask, templatesTask, unitsTask, entriesTask);

            var plannedFields = plannedFieldsTask.Result;
            var fieldTemplates = templatesTask.Result;
            var units = unitsTask.Result;
            var entries = entriesTask.Result;

            var fieldTemplateLookup = fieldTemplates
                .Where(t => plannedFields.Any(p => p.FieldTemplateId == t.Id))
                .ToDictionary(t => t.Id);

            var unitLookup = units.ToDictionary(u => u.Id);

            foreach (var plan in plannedFields)
            {
                if (!fieldTemplateLookup.TryGetValue(plan.FieldTemplateId, out var template))
                    continue;

                var entry = entries.FirstOrDefault(e => e.FieldTemplateId == plan.FieldTemplateId);

                // Skip if field deleted and no corresponding entry exists
                if (plan.IsDeleted && entry == null)
                    continue;

                template.Unit = unitLookup.TryGetValue(template.UnitId, out var unit)
                    ? unit
                    : new FieldUnit();

                Fields.Add(new DailyEntry
                {
                    Id = entry?.Id ?? 0,
                    FieldTemplate = template,
                    FieldTemplateId = template.Id,
                    UserId = userId,
                    Date = FormDate,
                    Value = entry?.Value ?? string.Empty
                });
            }
        }


        private async Task SubmitAsync()
        {
            foreach (var entry in Fields)
            {
                await _repository.SaveDailyEntryAsync(entry);
            }

            await Shell.Current.DisplayAlert("Success", "Daily entry submitted!", "OK");
            
            await Shell.Current.Navigation.PopAsync();
        }
    }

}
