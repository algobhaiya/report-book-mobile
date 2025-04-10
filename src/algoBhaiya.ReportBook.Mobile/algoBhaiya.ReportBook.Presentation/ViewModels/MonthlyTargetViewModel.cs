using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class MonthlyTargetFieldViewModel : INotifyPropertyChanged
    {
        public int FieldTemplateId { get; set; }
        public string FieldName { get; set; }
        public string ValueType { get; set; }
        public string UnitName { get; set; }

        private string _targetValue;
        public string TargetValue
        {
            get => _targetValue;
            set
            {
                if (_targetValue != value)
                {
                    _targetValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MonthlyTargetViewModel : INotifyPropertyChanged
    {
        private readonly IMonthlyTargetRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlyTargetFieldViewModel> Fields { get; set; } = new();
        public int SelectedMonth { get; set; } = DateTime.Today.Month;
        public int SelectedYear { get; set; } = DateTime.Today.Year;

        public Command LoadCommand { get; }
        public Command SaveCommand { get; }

        public MonthlyTargetViewModel(
            IServiceProvider serviceProvider,
            IMonthlyTargetRepository repository)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            LoadCommand = new Command(async () => await LoadTargetsAsync());
            SaveCommand = new Command(async () => await SaveTargetsAsync());

            LoadCommand.Execute(null);
        }

        public async Task LoadTargetsAsync()
        {
            Fields.Clear();
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) return;

            var templates = await _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>().GetAllAsync();
            var units = await _serviceProvider.GetRequiredService<IRepository<FieldUnit>>().GetAllAsync();
            var targets = await _repository.GetMonthlyTargetsAsync(userId, SelectedMonth, SelectedYear);

            foreach (var template in templates)
            {
                var unit = units.FirstOrDefault(u => u.Id == template.UnitId);
                var target = targets.FirstOrDefault(t => t.FieldTemplateId == template.Id);

                Fields.Add(new MonthlyTargetFieldViewModel
                {
                    FieldTemplateId = template.Id,
                    FieldName = template.FieldName,
                    ValueType = template.ValueType,
                    UnitName = unit?.UnitName ?? "",
                    TargetValue = target?.TargetValue ?? ""
                });
            }
        }

        public async Task SaveTargetsAsync()
        {
            int userId = Preferences.Get("CurrentUserId", -1);

            foreach (var field in Fields)
            {
                await _repository.SaveMonthlyTargetAsync(new MonthlyTarget
                {
                    UserId = userId,
                    FieldTemplateId = field.FieldTemplateId,
                    Month = SelectedMonth,
                    Year = SelectedYear,
                    TargetValue = field.TargetValue
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
