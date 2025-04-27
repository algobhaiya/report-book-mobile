using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

        public byte FieldOrder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MonthlyTargetViewModel : INotifyPropertyChanged
    {
        private readonly IMonthlyTargetRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlyTargetFieldViewModel> Fields { get; set; } = new();

        public Command SubmitCommand { get; }

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

        private string _currentMonthLabel;
        public string CurrentMonthLabel
        {
            get => _currentMonthLabel;
            set
            {
                if (_currentMonthLabel != value)
                {
                    _currentMonthLabel = value;
                    OnPropertyChanged(nameof(CurrentMonthLabel));
                }
            }
        }

        public bool CanSubmit => !IsReadOnly;
        private byte _loggedInUser = 0;

        public MonthlyTargetViewModel(
            IServiceProvider serviceProvider,
            IMonthlyTargetRepository repository)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            SubmitCommand = new Command(async () => await SaveTargetsAsync());

            _loggedInUser = (byte)Preferences.Get("CurrentUserId", 0);

            LoadTargetsAsync(DateTime.Today.Year, DateTime.Today.Month);
        }

        public async Task LoadTargetsAsync(int year, int month)
        {
            Fields.Clear();
            
            if (_loggedInUser == 0) return;

            IsReadOnly = IsNonEditableMonth(year, month);

            var templates = await _serviceProvider
                .GetRequiredService<IRepository<FieldTemplate>>()
                .GetListAsync(t => t.UserId == _loggedInUser);

            var units = await _serviceProvider
                .GetRequiredService<IRepository<FieldUnit>>()
                .GetAllAsync();

            var targets = await _repository.GetMonthlyTargetsAsync(_loggedInUser, year, month);

            if (IsReadOnly)
            {
                // Based on Fixed template
                targets = targets
                    .Where(t => t.IsDeleted == false)
                    .OrderBy(t => t.FieldOrder)
                    .ToList();

                foreach (var item in targets)
                {
                    var template = templates.FirstOrDefault(t => t.Id == item.FieldTemplateId);
                    var unit = units.FirstOrDefault(u => u.Id == template.UnitId);                  

                    Fields.Add(new MonthlyTargetFieldViewModel
                    {
                        FieldTemplateId = item.FieldTemplateId,
                        FieldName = template.FieldName,
                        ValueType = template.ValueType,
                        UnitName = unit?.UnitName ?? "",
                        TargetValue = item?.TargetValue ?? ""
                    });
                }
            }
            else
            {
                // Based on Dynamic current template
                templates = templates
                    .Where(t => 
                        t.IsDeleted == false && 
                        t.IsEnabled == true)
                    .OrderBy(t => t.FieldOrder);

                foreach (var template in templates)
                {
                    var unit = units.FirstOrDefault(u => u.Id == template.UnitId);
                    var target = targets.FirstOrDefault(t => t.FieldTemplateId == template.Id);

                    Fields.Add(new MonthlyTargetFieldViewModel
                    {
                        FieldTemplateId = template.Id,
                        FieldName = template.FieldName,
                        ValueType = template.ValueType,
                        FieldOrder = template.FieldOrder,
                        UnitName = unit?.UnitName ?? "",
                        TargetValue = target?.TargetValue ?? ""
                    });
                }
            }

            CurrentMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";           
        }

        public async Task SaveTargetsAsync()
        {
            byte userId = (byte)Preferences.Get("CurrentUserId", 0);
            if (userId == 0) return;

            foreach (var field in Fields)
            {
                await _repository.SaveMonthlyTargetAsync(new MonthlyTarget
                {
                    UserId = userId,
                    FieldTemplateId = field.FieldTemplateId,
                    Month = (byte)DateTime.Today.Month,
                    Year = DateTime.Today.Year,
                    TargetValue = field.TargetValue,
                    FieldOrder = field.FieldOrder
                });
            }

            await Shell.Current.DisplayAlert("Success", "Plan submitted!", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #region Helpers
        
        private bool IsNonEditableMonth(int year, int month)
        {
            return !(year == DateTime.Today.Year &&
                    month == DateTime.Today.Month);
        }

        #endregion
    }

}
