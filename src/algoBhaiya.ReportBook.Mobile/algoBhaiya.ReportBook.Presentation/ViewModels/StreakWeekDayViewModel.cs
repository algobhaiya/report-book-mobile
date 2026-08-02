using System.ComponentModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class StreakWeekDayViewModel : INotifyPropertyChanged
    {
        private string _dayLabel = string.Empty;
        public string DayLabel
        {
            get => _dayLabel;
            set
            {
                if (_dayLabel != value)
                {
                    _dayLabel = value;
                    OnPropertyChanged(nameof(DayLabel));
                }
            }
        }

        private int _filledCount;
        public int FilledCount
        {
            get => _filledCount;
            set
            {
                if (_filledCount != value)
                {
                    _filledCount = value;
                    OnPropertyChanged(nameof(FilledCount));
                }
            }
        }

        private double _barProgress;
        public double BarProgress
        {
            get => _barProgress;
            set
            {
                if (Math.Abs(_barProgress - value) > 0.0001)
                {
                    _barProgress = value;
                    OnPropertyChanged(nameof(BarProgress));
                }
            }
        }

        private double _barHeight = 29;
        public double BarHeight
        {
            get => _barHeight;
            set
            {
                if (Math.Abs(_barHeight - value) > 0.0001)
                {
                    _barHeight = value;
                    OnPropertyChanged(nameof(BarHeight));
                }
            }
        }

        private bool _isToday;
        public bool IsToday
        {
            get => _isToday;
            set
            {
                if (_isToday != value)
                {
                    _isToday = value;
                    OnPropertyChanged(nameof(IsToday));
                }
            }
        }

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
