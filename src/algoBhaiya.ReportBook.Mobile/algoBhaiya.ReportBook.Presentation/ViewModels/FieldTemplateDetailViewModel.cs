using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class FieldTemplateDetailViewModel : INotifyPropertyChanged
    {
        private readonly NavigationDataService _navDataService;
        private readonly IRepository<FieldTemplate> _repository;
        private readonly IRepository<FieldUnit> _unitRepository;        

        private readonly Action<FieldTemplate, FieldTemplate> _onSave;
        public  Action onModalClose;
        public ObservableCollection<string> DisplayUnitNames { get; } = new();
        private byte _loggedInUser = 0;

        private string _fieldName;
        public string FieldName
        {
            get => string.IsNullOrWhiteSpace(_fieldName) ? string.Empty : _fieldName.Trim();
            set
            {
                if (_fieldName != value)
                {
                    _fieldName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedUnitName;
        public string SelectedUnitName
        {
            get => _selectedUnitName;
            set
            {
                if (_selectedUnitName != value)
                {
                    _selectedUnitName = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte _fieldOrder;
        public byte FieldOrder
        {
            get => _fieldOrder;
            set
            {
                if (_fieldOrder != value)
                {
                    _fieldOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SubmitCommand { get; }
        
        public FieldTemplate TappedField { get; set; }

        public FieldTemplateDetailViewModel(
            IRepository<FieldTemplate> repository,
            IRepository<FieldUnit> unitRepository,
            NavigationDataService navigationDataService)
        {
            _repository = repository;
            _unitRepository = unitRepository;
            _navDataService = navigationDataService;

            SubmitCommand = new Command(async () => await SubmitAsync());

            _loggedInUser = (byte)Preferences.Get("CurrentUserId", 0);
            
            var onSaveAction = _navDataService.Get<Action<FieldTemplate, FieldTemplate>>(Constants.Constants.FieldTemplate.Action_OnUnitSaved);
            if (onSaveAction != null)
            {
                _onSave = onSaveAction;
            }

            _navDataService.Remove(Constants.Constants.FieldTemplate.Action_OnUnitSaved);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public async Task PopulateDataAsync()
        {
            // Load units
            var unitList = (await _unitRepository
                .GetListAsync(u => u.IsDeleted == false)
                )
                .OrderBy(u => u.UnitName)
                .Select(u => u.UnitName)
                .ToList();

            foreach (var unit in unitList)
                DisplayUnitNames.Add(unit);

            // Selected unit
            var fieldTemplate = _navDataService.Get<FieldTemplate>(Constants.Constants.FieldTemplate.Item_ToEdit);
            
            AssignEntryAsync(fieldTemplate);

            _navDataService.Remove(Constants.Constants.FieldTemplate.Item_ToEdit);
        }

        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(FieldName) || string.IsNullOrWhiteSpace(SelectedUnitName))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                return;
            }

            var backendUnit = await _unitRepository
                .GetFirstOrDefaultAsync(u => 
                    u.UnitName == SelectedUnitName &&
                    u.IsDeleted == false);

            // If different name, then check duplicate.
            if (TappedField.FieldName != FieldName)
            {
                // validate fieldName.
                var duplicateField = await _repository
                    .GetFirstOrDefaultAsync(
                        f => f.FieldName == FieldName && 
                        f.UserId == _loggedInUser &&
                        f.IsDeleted == false);

                if (duplicateField != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Duplicate Name",
                        $"The field \"{FieldName}\" already exists. Please choose a different name.",
                        "OK");
                    return;
                }

                await SaveAsync(backendUnit);
            } 
            else if (HasFieldValueChanged(backendUnit))
            {
                await SaveAsync(backendUnit);
            } 
            else
            {
                // Skip updating.
            }

            onModalClose?.Invoke();           
        }

        private async Task SaveAsync(FieldUnit backendUnit)
        {
            var oldField = await DeleteFieldAsync(TappedField.FieldName, TappedField.UnitId);

            var newField = await AddFieldAsync(FieldName, backendUnit);

            oldField.Unit = newField.Unit = backendUnit;

            await ReplaceByNewFieldAsync(oldField, newField);

            _onSave?.Invoke(oldField, newField); // Notify list page
        }

        private async void AssignEntryAsync(FieldTemplate? fieldTemplate)
        {
            if (fieldTemplate == null)
            {
                FieldName = string.Empty;
                SelectedUnitName = string.Empty;
                
                // Take the max value (as default)
                var maxOrder = (await _repository
                    .GetListAsync(f =>
                        f.UserId == _loggedInUser &&
                        f.IsEnabled == true &&
                        f.IsDeleted == false)
                    )
                    .Select(f => f.FieldOrder)
                    .OrderByDescending(f => f)
                    .FirstOrDefault();

                FieldOrder = byte.Min(maxOrder, byte.MaxValue);
            }
            else
            {
                FieldName = fieldTemplate.FieldName;
                SelectedUnitName = fieldTemplate.Unit.UnitName;
                FieldOrder = fieldTemplate.FieldOrder;
            }

            TappedField = fieldTemplate ?? new FieldTemplate();
        }

        private async Task<FieldTemplate> DeleteFieldAsync(string fieldName, byte unitId)
        {
            if (fieldName == null || unitId == 0)
                return new FieldTemplate();
            
            var field = await _repository.GetFirstOrDefaultAsync(
                    f => f.FieldName == fieldName &&
                         f.UserId == _loggedInUser &&
                         f.UnitId == unitId);

            if (field != null)
            {
                field.IsDeleted = true;

                await _repository.UpdateAsync(field);
            }

            return field ?? new FieldTemplate();
        }

        private async Task<FieldTemplate> AddFieldAsync(string fieldName, FieldUnit backendUnit)
        {
            var field = await _repository.GetFirstOrDefaultAsync(
                    f => f.FieldName == fieldName &&
                         f.UserId == _loggedInUser &&
                         f.UnitId == backendUnit.Id);

            if (field != null)
            {
                field.IsDeleted = false;
                field.FieldOrder = FieldOrder;

                await _repository.UpdateAsync(field);
            }
            else
            {
                field = new FieldTemplate
                {
                    FieldName = FieldName,                    
                    UnitId = backendUnit.Id,
                    FieldOrder = FieldOrder,
                    UserId = _loggedInUser
                };

                await _repository.AddAsync(field);
            }

            return field;
        }

        private async Task ReplaceByNewFieldAsync(FieldTemplate oldField, FieldTemplate newField)
        {
            // soft delete old field of current monthly target.
            // soft delete flag on / Add new field in current monthly target.
            await Task.CompletedTask;
        }

        private bool HasFieldValueChanged(FieldUnit backendUnit)
        {
            return TappedField.UnitId != backendUnit.Id ||
                   TappedField.FieldOrder != FieldOrder;
        }
    }

}
