using algoBhaiya.ReportBook.Core.Entities;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryFieldViewModel : INotifyPropertyChanged
    {
        public int FieldTemplateId { get; set; }
        public string FieldName { get; set; }
        public string ValueType { get; set; } // "int", "double", "bool"
        public string UnitName { get; set; }
        public FieldUnit Unit { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }

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
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
