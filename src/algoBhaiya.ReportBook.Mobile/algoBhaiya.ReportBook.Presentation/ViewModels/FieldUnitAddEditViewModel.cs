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

        private readonly Action<FieldUnit> _onSubmit;
        public ObservableCollection<string> DisplayTypes { get; } = new();

        private string _unitName;
        public string UnitName
        {
            get => _unitName;
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
            { "Whole Number", "int" },
            { "Decimal Number", "double" },
            { "Yes/No (Checkbox)", "bool" }
        };
        
        public FieldUnit TappedUnit { get; set; }

        public FieldUnitAddEditViewModel(
            IRepository<FieldUnit> repository,
            NavigationDataService navigationDataService)
        {
            _repository = repository;
            _navDataService = navigationDataService;

            SubmitCommand = new Command(async () => await SubmitAsync());

            foreach (var key in _typeMap.Keys)
                DisplayTypes.Add(key);

            var unit = _navDataService.Get<FieldUnit>("FieldUnitToEdit");
            AssignEntryAsync(unit);

            var onSaveAction = _navDataService.Get<Action<FieldUnit>>("OnUnitSaved");
            if (onSaveAction != null)
            {
                _onSubmit = onSaveAction;
            }

            _navDataService.Remove("FieldUnitToEdit");
            _navDataService.Remove("OnUnitSaved");
        }

        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(UnitName) || string.IsNullOrWhiteSpace(SelectedDisplayType))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                return;
            }

            var backendType = _typeMap[SelectedDisplayType];

            var unit = await _repository.GetFirstOrDefaultAsync(
                u => u.UnitName == TappedUnit.UnitName &&
                     u.ValueType == TappedUnit.ValueType);

            if (unit != null)
            {
                unit.UnitName = UnitName.Trim();
                unit.ValueType = backendType;

                await _repository.UpdateAsync(unit);
            }
            else
            {
                unit = new FieldUnit
                {
                    UnitName = UnitName.Trim(),
                    ValueType = backendType
                };

                await _repository.AddAsync(unit);
            }

            _onSubmit?.Invoke(unit); // Notify list page

            await Shell.Current.Navigation.PopModalAsync();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
    }

}
