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

        private string _unitName;
        public string UnitName
        {
            get => _unitName.Trim();
            set
            {
                if (_unitName != value)
                {
                    _unitName = value;
                    OnPropertyChanged();
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
                }
            }
        }

        public ICommand SubmitCommand { get; }

        private readonly Dictionary<string, string> _typeMap = new()
        {
            { "Number", "double" },
            { "Yes/No (Checkbox)", "bool" }
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
            if (string.IsNullOrWhiteSpace(UnitName) || string.IsNullOrWhiteSpace(SelectedDisplayType))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                return;
            }

            var backendType = _typeMap[SelectedDisplayType];

            // If different name, then check duplicate.
            if (TappedUnit.UnitName != UnitName)
            {
                // validate unitName.
                var duplicateUnit = await _repository.GetFirstOrDefaultAsync(
                        u => u.UnitName == UnitName && u.IsDeleted == false);

                if (duplicateUnit != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Duplicate Name",
                        $"The unit \"{UnitName}\" already exists. Please choose a different name.",
                        "OK");
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
        }

        private string GetDisplayValueType(string backendValueType)
        {
            return backendValueType switch
            {
                "int" => "Whole Number",
                "double" => "Decimal Number",
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
