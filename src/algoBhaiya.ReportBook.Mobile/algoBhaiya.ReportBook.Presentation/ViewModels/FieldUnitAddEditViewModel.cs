using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class FieldUnitAddEditViewModel : INotifyPropertyChanged
    {
        private readonly NavigationDataService _navDataService;
        private readonly IRepository<FieldUnit> _repository;
        private readonly IRepository<FieldTemplate> _templateRepository;

        private readonly Action<FieldUnit, FieldUnit> _onSave;
        public  Action onModalClose;
        public ObservableCollection<string> DisplayTypes { get; } = new();
        private bool _isSubmitting;

        private readonly string _newModeTitle = "Add Unit";
        private readonly string _editModeTitle = "Edit Unit";
        private readonly string _newModeSubtitle = "Choose a name and value type for this unit.";
        private readonly string _editModeSubtitle = "Update the unit name or value type.";
        private readonly string _saveButtonText = "Save";
        private readonly string _updateButtonText = "Update";

        private string _unitName;
        public string UnitName
        {
            get => _unitName?.Trim() ?? string.Empty;
            set
            {
                if (_unitName != value)
                {
                    _unitName = value;
                    OnPropertyChanged();
                    if (!string.IsNullOrWhiteSpace(_unitName))
                    {
                        UnitNameError = string.Empty;
                    }
                }
            }
        }

        private string _selectedDisplayType;
        public string SelectedDisplayType
        {
            get => _selectedDisplayType;
            set
            {
                if (_selectedDisplayType != value)
                {
                    _selectedDisplayType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasDisplayTypeError));
                }
            }
        }

        public ICommand SubmitCommand { get; }
        public string PageTitle => IsEditMode ? _editModeTitle : _newModeTitle;
        public string PageSubtitle => IsEditMode ? _editModeSubtitle : _newModeSubtitle;
        public string SubmitButtonText => IsEditMode ? _updateButtonText : _saveButtonText;
        public bool IsEditMode => TappedUnit != null && TappedUnit.Id > 0;
        public bool IsSubmitting
        {
            get => _isSubmitting;
            private set
            {
                if (_isSubmitting != value)
                {
                    _isSubmitting = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _unitNameError;
        public string UnitNameError
        {
            get => _unitNameError;
            private set
            {
                if (_unitNameError != value)
                {
                    _unitNameError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasUnitNameError));
                }
            }
        }

        private string _displayTypeError;
        public string DisplayTypeError
        {
            get => _displayTypeError;
            private set
            {
                if (_displayTypeError != value)
                {
                    _displayTypeError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasDisplayTypeError));
                }
            }
        }

        public bool HasUnitNameError => !string.IsNullOrWhiteSpace(UnitNameError);
        public bool HasDisplayTypeError => !string.IsNullOrWhiteSpace(DisplayTypeError);

        private readonly Dictionary<string, string> _typeMap = new()
        {
            { "Number", Constants.Constants.UnitType.Double },
            { "Yes/No (Checkbox)", Constants.Constants.UnitType.Bool }
        };
        
        public FieldUnit TappedUnit { get; set; }

        public FieldUnitAddEditViewModel(
            IRepository<FieldUnit> repository,
            IRepository<FieldTemplate> templateRepository,
            NavigationDataService navigationDataService)
        {
            _repository = repository;
            _templateRepository = templateRepository;
            _navDataService = navigationDataService;

            SubmitCommand = new Command(async () => await SubmitAsync());

            foreach (var key in _typeMap.Keys)
                DisplayTypes.Add(key);

            var unit = _navDataService.Get<FieldUnit>(Constants.Constants.FieldUnit.Item_ToEdit);
            AssignEntryAsync(unit);
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(PageSubtitle));
            OnPropertyChanged(nameof(SubmitButtonText));
            OnPropertyChanged(nameof(IsEditMode));

            var onSaveAction = _navDataService.Get<Action<FieldUnit, FieldUnit>>(Constants.Constants.FieldUnit.Action_OnUnitSaved);
            if (onSaveAction != null)
            {
                _onSave = onSaveAction;
            }

            _navDataService.Remove(Constants.Constants.FieldUnit.Item_ToEdit);
            _navDataService.Remove(Constants.Constants.FieldUnit.Action_OnUnitSaved);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async Task SubmitAsync()
        {
            if (IsSubmitting)
            {
                return;
            }

            IsSubmitting = true;
            try
            {
                var isValid = ValidateFields();
                if (!isValid)
                {
                    return;
                }

                var backendType = _typeMap[SelectedDisplayType];
                var normalizedUnitName = UnitName.Trim();

                // If different name, then check duplicate.
                if (!string.Equals(TappedUnit.UnitName, normalizedUnitName, StringComparison.Ordinal))
                {
                    // validate unitName.
                    var duplicateUnit = await _repository.GetFirstOrDefaultAsync(
                            u => u.UnitName == normalizedUnitName && u.IsDeleted == false);

                    if (duplicateUnit != null)
                    {
                        UnitNameError = $"The unit \"{normalizedUnitName}\" already exists. Please choose a different name.";
                        return;
                    }

                    await SaveAsync(backendType);
                } 
                else if (HasFieldValueChanged(backendType))
                {
                    await SaveAsync(backendType);
                } 
                else
                {
                    // Skip updating.
                }

                onModalClose?.Invoke();
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private bool ValidateFields()
        {
            var isValid = true;

            if (!ValidateUnitName())
            {
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SelectedDisplayType))
            {
                DisplayTypeError = "Please select a value type.";
                isValid = false;
            }
            else
            {
                DisplayTypeError = string.Empty;
            }

            return isValid;
        }

        private bool ValidateUnitName()
        {
            var normalizedUnitName = UnitName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedUnitName))
            {
                UnitNameError = "Unit name is required.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync(string backendType)
        {
            var oldUnit = await DeleteUnitAsync(TappedUnit.UnitName, TappedUnit.ValueType);

            var newUnit = await AddUnitAsync(UnitName, backendType);

            await ReplaceByNewUnitAsync(oldUnit, newUnit);

            _onSave?.Invoke(oldUnit, newUnit); // Notify list page
        }

        private void AssignEntryAsync(FieldUnit? fieldUnit)
        {
            if (fieldUnit == null)
            {
                UnitName = string.Empty;
                SelectedDisplayType = string.Empty;
            }
            else
            {
                UnitName = fieldUnit.UnitName;
                SelectedDisplayType = GetDisplayValueType(fieldUnit.ValueType);
            }
            
            TappedUnit = fieldUnit ?? new FieldUnit();
            UnitNameError = string.Empty;
            DisplayTypeError = string.Empty;
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(PageSubtitle));
            OnPropertyChanged(nameof(SubmitButtonText));
            OnPropertyChanged(nameof(IsEditMode));
        }

        private string GetDisplayValueType(string backendValueType)
        {
            return backendValueType switch
            {
                "double" => "Number",
                "bool" => "Yes/No (Checkbox)",
                _ => "Unknown"
            };
        }
        
        private async Task<FieldUnit> DeleteUnitAsync(string unitName, string valueType)
        {
            if (unitName == null || valueType == null)
                return new FieldUnit();
            
            var unit = await _repository.GetFirstOrDefaultAsync(
                    u => u.UnitName == unitName &&
                         u.ValueType == valueType);

            if (unit != null)
            {
                unit.IsDeleted = true;

                await _repository.UpdateAsync(unit);
            }

            return unit ?? new FieldUnit();
        }

        private async Task<FieldUnit> AddUnitAsync(string unitName, string valueType)
        {
            var unit = await _repository.GetFirstOrDefaultAsync(
                    u => u.UnitName == unitName &&
                         u.ValueType == valueType);

            if (unit != null)
            {
                unit.IsDeleted = false;

                await _repository.UpdateAsync(unit);
            }
            else
            {
                unit = new FieldUnit
                {
                    UnitName = UnitName,
                    ValueType = valueType
                };

                await _repository.AddAsync(unit);
            }

            return unit;
        }

        private async Task ReplaceByNewUnitAsync(FieldUnit oldUnit, FieldUnit newUnit)
        {
            var templates = await _templateRepository.GetListAsync(t => t.UnitId == oldUnit.Id);
            
            foreach (var template in templates)
            {
                template.UnitId = newUnit.Id;
            }

            await _templateRepository.UpdateAsync(templates);
        }

        private bool HasFieldValueChanged(string valueType)
        {
            return TappedUnit.ValueType != valueType;
        }
    }

}
