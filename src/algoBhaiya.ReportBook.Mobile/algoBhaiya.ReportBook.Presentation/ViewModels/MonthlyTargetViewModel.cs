using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Globalization;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class MonthlyTargetFieldViewModel : BaseViewModel
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
    }

    public class MonthlyTargetViewModel : BaseViewModel
    {
        private readonly IMonthlyTargetRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlyTargetFieldViewModel> Fields { get; set; } = new();

        public Command SubmitCommand { get; }
        public Command PreviousMonthCommand { get; }
        public Command NextMonthCommand { get; }

        private bool _isLoadingData = false;
        public bool IsLoadingMonth
        {
            get => _isLoadingData;
            private set
            {
                if (_isLoadingData != value)
                {
                    _isLoadingData = value;
                    OnPropertyChanged();
                    PreviousMonthCommand.ChangeCanExecute();
                    NextMonthCommand.ChangeCanExecute();
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

        private DateTime _currentMonthDate;
        private bool _hasLoadedMonth;
        public bool HasLoadedMonth => _hasLoadedMonth;

        public bool CanSubmit => !IsReadOnly;
        private byte _loggedInUser = 0;

        private DateTime _selectedItemDate = DateTime.Today;

        public MonthlyTargetViewModel(
            IServiceProvider serviceProvider,
            IMonthlyTargetRepository repository)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            SubmitCommand = new Command(async () => await SaveTargetsAsync());
            PreviousMonthCommand = new Command(async () => await NavigateMonthAsync(-1), () => !_isLoadingData);
            NextMonthCommand = new Command(async () => await NavigateMonthAsync(1), () => !_isLoadingData);

            _loggedInUser = (byte)Preferences.Get("CurrentUserId", 0);
        }

        public async Task LoadTargetsAsync(int year, int month)
        {
            if (IsLoadingMonth)
            {
                return;
            }

            IsLoadingMonth = true;
            Fields.Clear();
            try
            {
                if (_loggedInUser == 0) return;

                _selectedItemDate = new DateTime(year, month, 1);
                _currentMonthDate = _selectedItemDate;
                _hasLoadedMonth = true;

                IsReadOnly = IsNonEditableMonth(year, month);

                var templatesTask = _serviceProvider
                    .GetRequiredService<IRepository<FieldTemplate>>()
                    .GetListAsync(t => t.UserId == _loggedInUser);

                var unitsTask = _serviceProvider
                    .GetRequiredService<IRepository<FieldUnit>>()
                    .GetAllAsync();

                var targetsTask = _repository.GetMonthlyTargetsAsync(_loggedInUser, year, month);

                await Task.WhenAll(templatesTask, unitsTask, targetsTask);

                var templates = templatesTask.Result;
                var units = unitsTask.Result;
                var targets = targetsTask.Result;

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
                        t.UserId == _loggedInUser &&
                        !t.IsDeleted && 
                        t.IsEnabled)
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
            finally
            {
                IsLoadingMonth = false;
            }
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
                    Month = (byte)_selectedItemDate.Month,
                    Year = _selectedItemDate.Year,
                    TargetValue = field.TargetValue,
                    FieldOrder = field.FieldOrder,
                    IsDeleted = false
                });
            }

            await Shell.Current.DisplayAlert("Success", "Plan submitted!", "OK");
        }

        public async Task RefreshTargetsAsync()
        {
            await LoadTargetsAsync(_selectedItemDate.Year, _selectedItemDate.Month);
        }

        #region Helpers

        private async Task NavigateMonthAsync(int offset)
        {
            if (_currentMonthDate == default || IsLoadingMonth)
            {
                return;
            }

            var targetMonth = _currentMonthDate.AddMonths(offset);
            await LoadTargetsAsync(targetMonth.Year, targetMonth.Month);
        }
        
        private bool IsNonEditableMonth(int year, int month)
        {
            var today = DateTime.Today;
            return (today.Year == year && today.Month > month) || today.Year > year;
        }

        #endregion
    }

}
