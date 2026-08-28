using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryViewModel : INotifyPropertyChanged
    {
        public DailyEntryFieldCollection Fields { get; } = new();

        private readonly Command _submitCommand;
        public ICommand SubmitCommand => _submitCommand;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LoadingDateTime { get; set; }
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
                    OnPropertyChanged(nameof(CanShowSubmit));
                    _submitCommand.ChangeCanExecute();
                }
            }
        }

        public bool IsDirty => Fields.DirtyCount > 0;

        private byte _maxEditableDayCount = 15;

        public bool CanSubmit => !IsReadOnly && IsDirty;
        public bool CanShowSubmit => !IsReadOnly;

        private readonly IDailyEntryRepository _repository;
        private readonly ITrackingStreakService _trackingStreakService;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationDataService _navDataService;
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        private bool _showCompletionCelebration;
        public bool ShowCompletionCelebration
        {
            get => _showCompletionCelebration;
            private set
            {
                if (_showCompletionCelebration != value)
                {
                    _showCompletionCelebration = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _cachedUserId;
        private DateTime _cachedLoadedDate = DateTime.MinValue;
        private Dictionary<int, FieldTemplate> _cachedTemplateLookup = new();
        private Dictionary<byte, FieldUnit> _cachedUnitLookup = new();
        private List<DailyEntryFieldViewModel> _fieldBlueprints = new();
        public void InvalidateCache()
        {
            _cachedUserId = 0;
            _cachedLoadedDate = DateTime.MinValue;
            _cachedTemplateLookup.Clear();
            _cachedUnitLookup.Clear();
            _fieldBlueprints.Clear();
            Fields.Clear();
            ShowCompletionCelebration = false;
        }

        public DailyEntryViewModel(
            IServiceProvider serviceProvider,
            IDailyEntryRepository repository,
            ITrackingStreakService trackingStreakService,
            NavigationDataService navDataService
            )
        {
            _repository = repository;
            _trackingStreakService = trackingStreakService;
            _serviceProvider = serviceProvider;
            _navDataService = navDataService;

            _maxEditableDayCount = (byte)Preferences.Get(Constants.Constants.Setting.ModificationDuration, 15);

            _submitCommand = new Command(async () => await SubmitAsync(), () => CanSubmit);
            Fields.DirtyCountChanged += OnDirtyCountChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task LoadFieldsAsync()
        {
            await _loadLock.WaitAsync();
            IsLoading = true;

            try
            {
                await LoadFieldsCoreAsync();
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }

        public async Task LoadFieldsForDateAsync(DateTime date)
        {
            if (_cachedUserId == (byte)Preferences.Get("CurrentUserId", 0)
                && _fieldBlueprints.Count > 0
                && _cachedLoadedDate.Year == date.Year
                && _cachedLoadedDate.Month == date.Month)
            {
                await RefreshFieldValuesAsync(date.Date);
                return;
            }

            _navDataService.Set(Constants.Constants.DailyEntry.Item_SelectedDate, date.Date);
            await LoadFieldsAsync();
        }

        public async Task RefreshFieldValuesAsync(DateTime date)
        {
            await _loadLock.WaitAsync();
            IsLoading = true;

            try
            {
                if (_cachedUserId != (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0)
                    || _fieldBlueprints.Count == 0
                    || _cachedLoadedDate.Year != date.Year
                    || _cachedLoadedDate.Month != date.Month)
                {
                    _navDataService.Set(Constants.Constants.DailyEntry.Item_SelectedDate, date.Date);
                    await LoadFieldsCoreAsync();
                    return;
                }

                await RefreshFieldValuesCoreAsync(date.Date);
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }

        public void HideCompletionCelebration()
        {
            ShowCompletionCelebration = false;
        }

        private async Task LoadFieldsCoreAsync()
        {
            byte userId = (byte)Preferences.Get("CurrentUserId", 0);
            if (userId == 0)
            {
                Fields.Clear();
                _fieldBlueprints.Clear();
                _cachedLoadedDate = DateTime.MinValue;
                return;
            }

            SetLoadingTime();
            FormDate = LoadingDateTime;
            IsReadOnly = (DateTime.Today - FormDate).Days > _maxEditableDayCount;

            var targetRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var templateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();
            var unitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

            var plannedFieldsTask = targetRepo
                .GetListAsync(f =>
                    f.UserId == userId &&
                    f.Month == FormDate.Month &&
                    f.Year == FormDate.Year);

            var entriesTask = _repository.GetEntriesForUserAndDateAsync(userId, FormDate);
            Task<IEnumerable<FieldTemplate>> templatesTask;
            Task<IEnumerable<FieldUnit>> unitsTask;

            if (_cachedUserId == userId && _cachedTemplateLookup.Count > 0 && _cachedUnitLookup.Count > 0)
            {
                templatesTask = Task.FromResult<IEnumerable<FieldTemplate>>(_cachedTemplateLookup.Values);
                unitsTask = Task.FromResult<IEnumerable<FieldUnit>>(_cachedUnitLookup.Values);
            }
            else
            {
                templatesTask = templateRepo.GetListAsync(f => f.UserId == userId);
                unitsTask = unitRepo.GetAllAsync();
            }

            await Task.WhenAll(plannedFieldsTask, templatesTask, unitsTask, entriesTask);

            var plannedFields = (await plannedFieldsTask).OrderBy(p => p.FieldOrder);
            var fieldTemplates = await templatesTask;
            var units = await unitsTask;
            var entries = await entriesTask;
            var entriesLookup = entries
                .GroupBy(e => e.FieldTemplateId)
                .ToDictionary(g => g.Key, g => g.First());

            _cachedUserId = userId;
            _cachedTemplateLookup = fieldTemplates.ToDictionary(t => t.Id);
            _cachedUnitLookup = units.ToDictionary(u => u.Id);
            _cachedLoadedDate = FormDate.Date;

            using (Fields.SuspendDirtyTracking())
            {
                Fields.Clear();
                _fieldBlueprints.Clear();

                foreach (var plan in plannedFields)
                {
                    if (!_cachedTemplateLookup.TryGetValue(plan.FieldTemplateId, out var template))
                        continue;

                    entriesLookup.TryGetValue(plan.FieldTemplateId, out var entry);

                    if (plan.IsDeleted && entry == null)
                        continue;

                    template.Unit = _cachedUnitLookup.TryGetValue(template.UnitId, out var unit)
                        ? unit
                        : new FieldUnit();

                    var fieldVm = new DailyEntryFieldViewModel
                    {
                        FieldTemplate = template,
                        FieldTemplateId = template.Id,
                        FieldName = template.FieldName,
                        ValueType = template.ValueType,
                        UnitName = template.Unit?.UnitName ?? string.Empty,
                        UserId = userId
                    };

                    fieldVm.ApplyEntry(entry, FormDate);
                    _fieldBlueprints.Add(fieldVm);
                    Fields.Add(fieldVm);
                }
            }
        }

        private async Task RefreshFieldValuesCoreAsync(DateTime date)
        {
            byte userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            if (userId == 0 || _fieldBlueprints.Count == 0)
            {
                _navDataService.Set(Constants.Constants.DailyEntry.Item_SelectedDate, date);
                await LoadFieldsCoreAsync();
                return;
            }

            FormDate = date;
            IsReadOnly = (DateTime.Today - FormDate).Days > _maxEditableDayCount;

            var entries = await _repository.GetEntriesForUserAndDateAsync(userId, date);
            var entriesLookup = entries
                .GroupBy(e => e.FieldTemplateId)
                .ToDictionary(g => g.Key, g => g.First());

            using (Fields.SuspendDirtyTracking())
            {
                for (var i = 0; i < _fieldBlueprints.Count; i++)
                {
                    var blueprint = _fieldBlueprints[i];
                    entriesLookup.TryGetValue(blueprint.FieldTemplateId, out var entry);
                    blueprint.RefreshValue(entry, date);
                }
            }

            _cachedLoadedDate = date;
        }

        private void SetLoadingTime()
        {
            DateTime? loadingDateTime = _navDataService.Get<DateTime>(Constants.Constants.DailyEntry.Item_SelectedDate);

            LoadingDateTime = loadingDateTime ?? DateTime.Today;

            _navDataService.Remove(Constants.Constants.DailyEntry.Item_SelectedDate);
        }

        private void OnDirtyCountChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(CanSubmit));
            _submitCommand.ChangeCanExecute();
        }

        private async Task SubmitAsync()
        {
            var isCompleted = IsEntryFullyCompletedFromFields();

            foreach (var entry in Fields)
            {
                await _repository.SaveDailyEntryAsync(new DailyEntry
                {
                    Id = entry.Id,
                    UserId = (byte)entry.UserId,
                    FieldTemplateId = entry.FieldTemplateId,
                    Date = entry.Date,
                    Value = entry.Value,
                    FieldTemplate = entry.FieldTemplate
                });
            }

            byte userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            if (userId != 0)
            {
                await _trackingStreakService.NotifyDailyEntryChangedAsync(userId, FormDate);
            }

            _navDataService.Set(Constants.Constants.DailyEntry.Action_RefreshListOnReturn, true);

            if (isCompleted)
            {
                ShowCompletionCelebration = true;
                await Task.Delay(1600);
                HideCompletionCelebration();
            }
            else
            {
                await Shell.Current.DisplayAlert("Success", "Daily entry submitted!", "OK");
            }

            await RefreshFieldValuesAsync(FormDate.Date);
        }

        private bool IsEntryFullyCompletedFromFields()
        {
            if (Fields.Count == 0)
            {
                return false;
            }

            return Fields.All(IsSaveableFieldValue);
        }

        private static bool IsSaveableFieldValue(DailyEntryFieldViewModel entry)
        {
            if (entry == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(entry.Value))
            {
                return false;
            }

            return !string.Equals(entry.Value, "0", StringComparison.Ordinal)
                && !string.Equals(entry.Value, "False", StringComparison.Ordinal);
        }
    }
}
