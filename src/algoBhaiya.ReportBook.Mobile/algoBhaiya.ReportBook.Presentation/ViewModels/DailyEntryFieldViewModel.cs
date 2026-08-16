using algoBhaiya.ReportBook.Core.Entities;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryFieldViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int FieldTemplateId { get; set; }
        public string FieldName { get; set; }
        public string ValueType { get; set; } // "int", "double", "bool"
        public string UnitName { get; set; }
        public FieldTemplate FieldTemplate { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }

        private string _originalValue = string.Empty;
        public string OriginalValue
        {
            get => _originalValue;
            set
            {
                if (_originalValue != value)
                {
                    _originalValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        public bool IsDirty => !string.Equals(Value ?? string.Empty, OriginalValue ?? string.Empty, StringComparison.Ordinal);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
