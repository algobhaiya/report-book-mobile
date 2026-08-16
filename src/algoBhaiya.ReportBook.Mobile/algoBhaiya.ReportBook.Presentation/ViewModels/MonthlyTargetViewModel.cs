using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        private string _originalTargetValue = string.Empty;
        public string OriginalTargetValue
        {
            get => _originalTargetValue;
            set
            {
                if (_originalTargetValue != value)
                {
                    _originalTargetValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        public bool IsDirty => !string.Equals(TargetValue ?? string.Empty, OriginalTargetValue ?? string.Empty, StringComparison.Ordinal);

        public byte FieldOrder { get; set; }
    }

    public class MonthlyTargetViewModel : BaseViewModel
    {
        private readonly IMonthlyTargetRepository _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlyTargetFieldViewModel> Fields { get; } = new();

        private readonly Command _submitCommand;
        public Command SubmitCommand => _submitCommand;
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
                    OnPropertyChanged(nameof(CanShowSubmit));
                    _submitCommand.ChangeCanExecute();
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

        private int _dirtyCount;
        public bool IsDirty => _dirtyCount > 0;
        public bool CanSubmit => !IsReadOnly && IsDirty;
        public bool CanShowSubmit => !IsReadOnly;
        private byte _loggedInUser = 0;

        private DateTime _selectedItemDate = DateTime.Today;

        public MonthlyTargetViewModel(
            IServiceProvider serviceProvider,
            IMonthlyTargetRepository repository)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            _submitCommand = new Command(async () => await SaveTargetsAsync(), () => CanSubmit);
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
            DetachFieldHandlers();
            Fields.Clear();
            _dirtyCount = 0;
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(CanSubmit));
            OnPropertyChanged(nameof(CanShowSubmit));
            _submitCommand.ChangeCanExecute();
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
                    targets = targets
                        .Where(t => t.IsDeleted == false)
                        .OrderBy(t => t.FieldOrder)
                        .ToList();

                    foreach (var item in targets)
                    {
                        var template = templates.FirstOrDefault(t => t.Id == item.FieldTemplateId);
                        if (template == null)
                        {
                            continue;
                        }

                        var unit = units.FirstOrDefault(u => u.Id == template.UnitId);

                        var field = new MonthlyTargetFieldViewModel
                        {
                            FieldTemplateId = item.FieldTemplateId,
                            FieldName = template.FieldName,
                            ValueType = template.ValueType,
                            UnitName = unit?.UnitName ?? string.Empty,
                            TargetValue = item.TargetValue ?? string.Empty,
                            OriginalTargetValue = item.TargetValue ?? string.Empty
                        };

                        field.PropertyChanged += OnFieldPropertyChanged;
                        Fields.Add(field);
                    }
                }
                else
                {
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

                        var field = new MonthlyTargetFieldViewModel
                        {
                            FieldTemplateId = template.Id,
                            FieldName = template.FieldName,
                            ValueType = template.ValueType,
                            FieldOrder = template.FieldOrder,
                            UnitName = unit?.UnitName ?? string.Empty,
                            TargetValue = target?.TargetValue ?? string.Empty,
                            OriginalTargetValue = target?.TargetValue ?? string.Empty
                        };

                        field.PropertyChanged += OnFieldPropertyChanged;
                        Fields.Add(field);
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

        private void OnFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MonthlyTargetFieldViewModel.TargetValue) &&
                e.PropertyName != nameof(MonthlyTargetFieldViewModel.OriginalTargetValue))
            {
                return;
            }

            _dirtyCount = Fields.Count(field => field.IsDirty);
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(CanSubmit));
            _submitCommand.ChangeCanExecute();
        }

        private void DetachFieldHandlers()
        {
            foreach (var field in Fields)
            {
                field.PropertyChanged -= OnFieldPropertyChanged;
            }
        }

        private bool IsNonEditableMonth(int year, int month)
        {
            var today = DateTime.Today;
            return (today.Year == year && today.Month > month) || today.Year > year;
        }

        #endregion
    }
}
