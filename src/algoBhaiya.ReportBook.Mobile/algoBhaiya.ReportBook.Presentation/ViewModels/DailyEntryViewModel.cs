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
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) return;

            FormDate = LoadingDateTime ?? DateTime.Today; // place to set effectiveDate
            IsReadOnly = (DateTime.Today - FormDate).Days > 14;
            
            var fieldTemplateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();
            var fieldUnitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

            var templates = (await fieldTemplateRepo.GetAllAsync()).ToList();
            var units = await fieldUnitRepo.GetAllAsync();
            var entries = await _repository.GetEntriesForUserAndDateAsync(userId, FormDate);

            foreach (var template in templates)
            {
                var unit = units.FirstOrDefault(u => u.Id == template.UnitId);
                var entry = entries.FirstOrDefault(e => e.FieldTemplateId == template.Id);

                template.Unit = unit ?? new FieldUnit();

                Fields.Add(new DailyEntry
                {
                    Id = entry?.Id ?? 0,
                    FieldTemplate = template,
                    FieldTemplateId = template.Id,
                    UserId = userId,
                    Date = FormDate,
                    Value = entry?.Value ?? ""
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
        }
    }

}
