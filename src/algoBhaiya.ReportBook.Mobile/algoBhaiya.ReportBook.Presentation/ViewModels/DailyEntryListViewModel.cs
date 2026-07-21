using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBook.Presentation.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DailyEntrySummaryViewModel> DailySummaries { get; } = new();
        
        private readonly IDailyEntryRepository _repository;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationDataService _navDataService;
        private const string RefreshFlagKey = Constants.Constants.DailyEntry.Action_RefreshListOnReturn;

        private string _selectedMonthLabel;
        public string SelectedMonthLabel 
        {
            get => _selectedMonthLabel;
            set
            {
                if (_selectedMonthLabel != value)
                {
                    _selectedMonthLabel = value;
                    OnPropertyChanged(nameof(SelectedMonthLabel));
                }
            }
        }

        private int _completedDaysCount;
        public int CompletedDaysCount
        {
            get => _completedDaysCount;
            private set
            {
                if (_completedDaysCount != value)
                {
                    _completedDaysCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CompletedDaysText));
                    OnPropertyChanged(nameof(HasCompletedDays));
                }
            }
        }

        private int _incompleteDaysCount;
        public int IncompleteDaysCount
        {
            get => _incompleteDaysCount;
            private set
            {
                if (_incompleteDaysCount != value)
                {
                    _incompleteDaysCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IncompleteDaysText));
                    OnPropertyChanged(nameof(HasIncompleteDays));
                }
            }
        }

        private int _pendingDaysCount;
        public int PendingDaysCount
        {
            get => _pendingDaysCount;
            private set
            {
                if (_pendingDaysCount != value)
                {
                    _pendingDaysCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PendingDaysText));
                    OnPropertyChanged(nameof(HasPendingDays));
                }
            }
        }

        public string CompletedDaysText => CompletedDaysCount > 0 ? $"Completed: {CompletedDaysCount}" : string.Empty;
        public string IncompleteDaysText => IncompleteDaysCount > 0 ? $"Partial: {IncompleteDaysCount}" : string.Empty;
        public string PendingDaysText => PendingDaysCount > 0 ? $"Pending: {PendingDaysCount}" : string.Empty;
        public bool HasCompletedDays => CompletedDaysCount > 0;
        public bool HasIncompleteDays => IncompleteDaysCount > 0;
        public bool HasPendingDays => PendingDaysCount > 0;

        private int _todayFilledCount;
        public int TodayFilledCount
        {
            get => _todayFilledCount;
            private set
            {
                if (_todayFilledCount != value)
                {
                    _todayFilledCount = value;
                    OnPropertyChanged(nameof(TodayProgressPercentText));
                    OnPropertyChanged(nameof(TodayProgressRatio));
                }
            }
        }

        private int _todayTotalCount;
        public int TodayTotalCount
        {
            get => _todayTotalCount;
            private set
            {
                if (_todayTotalCount != value)
                {
                    _todayTotalCount = value;
                    OnPropertyChanged(nameof(TodayProgressPercentText));
                    OnPropertyChanged(nameof(TodayProgressRatio));
                    OnPropertyChanged(nameof(HasTodayData));
                }
            }
        }

        public bool HasTodayData => TodayTotalCount > 0;
        public double TodayProgressRatio => TodayTotalCount > 0
            ? Math.Clamp((double)TodayFilledCount / TodayTotalCount, 0d, 1d)
            : 0d;
        public string TodayProgressPercentText => HasTodayData
            ? $"{TodayProgressRatio:P0}"
            : "0%";

        private Color _todayProgressColor = Color.FromArgb("#8CBFEA");
        public Color TodayProgressColor
        {
            get => _todayProgressColor;
            private set
            {
                if (_todayProgressColor != value)
                {
                    _todayProgressColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private Color _todayTrackColor = Color.FromArgb("#FFF8E1");
        public Color TodayTrackColor
        {
            get => _todayTrackColor;
            private set
            {
                if (_todayTrackColor != value)
                {
                    _todayTrackColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _todayStatusText = "No entry yet";
        public string TodayStatusText
        {
            get => _todayStatusText;
            private set
            {
                if (_todayStatusText != value)
                {
                    _todayStatusText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _todayStatusSupportText = "Start now";
        public string TodayStatusSupportText
        {
            get => _todayStatusSupportText;
            private set
            {
                if (_todayStatusSupportText != value)
                {
                    _todayStatusSupportText = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _selectedMonthDate = DateTime.Today;
        public DateTime SelectedMonthDate => _selectedMonthDate;
        private bool _isCurrentMonth = true;
        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            private set
            {
                if (_isCurrentMonth != value)
                {
                    _isCurrentMonth = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNavigating = false;
        public ICommand OpenEntryCommand { get; }
        public ICommand RefreshCommand { get; }
        public bool IsRefreshRequested => _navDataService.Get<bool>(RefreshFlagKey);

        public DailyEntryListViewModel(
            IDailyEntryRepository repository,
            IServiceProvider serviceProvider,
            NavigationDataService navDataService)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;
            _navDataService = navDataService;

            OpenEntryCommand = new Command<DailyEntrySummaryViewModel>(async (selectedItem) =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

                try
                {
                    _isNavigating = true;

                    // Passing the LoadingDateTime
                    _navDataService.Set(Constants.Constants.DailyEntry.Item_SelectedDate, selectedItem.Date);

                    var dailyEntryPage = _serviceProvider.GetRequiredService<DailyEntryPage>();
                    await Shell.Current.Navigation.PushAsync(dailyEntryPage);
                }
                finally
                {
                    _isNavigating = false;
                }
            });

            RefreshCommand = new Command(async () => await RefreshDailyEntriesAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private async Task LoadDailySummariesAsync(int year, int month)
        {
            DailySummaries.Clear();
            byte userId = (byte) Preferences.Get("CurrentUserId", 0);
            if (userId == 0)
            {
                _selectedMonthDate = new DateTime(year, month, 1);
                SelectedMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
                IsCurrentMonth = _selectedMonthDate.Year == DateTime.Today.Year
                    && _selectedMonthDate.Month == DateTime.Today.Month;
                CompletedDaysCount = 0;
                IncompleteDaysCount = 0;
                PendingDaysCount = 0;
                TodayFilledCount = 0;
                TodayTotalCount = 0;
                TodayProgressColor = Color.FromArgb("#8CBFEA");
                TodayTrackColor = Color.FromArgb("#FFF8E1");
                TodayStatusText = "No entry yet";
                TodayStatusSupportText = "Start now";
                return;
            }

            _selectedMonthDate = new DateTime(year, month, 1);
            IsCurrentMonth = _selectedMonthDate.Year == DateTime.Today.Year
                && _selectedMonthDate.Month == DateTime.Today.Month;

            SelectedMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
            
            var entries = await _repository.GetMonthlyEntrySummaryAsync(userId, year, month);
            var completedDays = 0;
            var incompleteDays = 0;
            var pendingDays = 0;
            DailyEntrySummaryViewModel todayEntry = null;
            
            foreach (var item in entries)
            {
                var isCompleted = item.FilledCount >= item.TotalFields && item.TotalFields > 0;
                var isIncomplete = item.FilledCount > 0 && item.FilledCount < item.TotalFields;
                var statusState = isCompleted
                    ? DailyEntryStatusState.Completed
                    : isIncomplete
                        ? DailyEntryStatusState.Incomplete
                        : DailyEntryStatusState.Pending;

                switch (statusState)
                {
                    case DailyEntryStatusState.Completed:
                        completedDays++;
                        break;
                    case DailyEntryStatusState.Incomplete:
                        incompleteDays++;
                        break;
                    default:
                        pendingDays++;
                        break;
                }

                var summaryItem = new DailyEntrySummaryViewModel
                {
                    Date = item.Date,
                    DateString = item.Date.ToString("dd MMMM yyyy"),
                    FilledCount = item.FilledCount,
                    TotalCount = item.TotalFields,
                    StatusText = isCompleted
                        ? $"Done: {item.FilledCount}/{item.TotalFields}"
                        : isIncomplete
                            ? $"Filled: {item.FilledCount}/{item.TotalFields}"
                            : $"Pending: 0/{item.TotalFields}",
                    StatusSupportText = isCompleted
                        ? "Great job"
                        : isIncomplete
                            ? "Keep going"
                            : "Start now",
                    StatusIcon = isCompleted
                        ? "green_check_mark.svg"
                        : isIncomplete
                            ? "partial_half_circle.svg"
                            : "pending_gray_status.svg"
                };

                if (item.Date.Date == DateTime.Today.Date)
                {
                    todayEntry = summaryItem;
                }

                DailySummaries.Add(summaryItem);
            }

            CompletedDaysCount = completedDays;
            IncompleteDaysCount = incompleteDays;
            PendingDaysCount = pendingDays;

            UpdateTodaySummary(todayEntry);
        }

        private void UpdateTodaySummary(DailyEntrySummaryViewModel todayEntry)
        {
        if (todayEntry == null)
        {
            TodayFilledCount = 0;
            TodayTotalCount = 0;
            TodayProgressColor = Color.FromArgb("#8CBFEA");
            TodayTrackColor = Color.FromArgb("#FFF8E1");
            TodayStatusText = "No entry yet";
            TodayStatusSupportText = "Start now";
            return;
        }

        TodayFilledCount = todayEntry.FilledCount;
        TodayTotalCount = todayEntry.TotalCount;
        TodayProgressColor = todayEntry.StatusBadgeBorderColor;
        TodayTrackColor = Color.FromArgb("#FFF8E1");
        TodayStatusText = todayEntry.StatusText;
        TodayStatusSupportText = todayEntry.StatusSupportText;
    }

        public async Task OpenEntryAsync(DateTime date)
        {
            var item = new DailyEntrySummaryViewModel { Date = date };
            OpenEntryCommand.Execute(item);
        }

        public async Task LoadEntriesMonthlyAsync(int year, int month)
        {
            await LoadDailySummariesAsync(year, month);            
        }

        public async Task RefreshDailyEntriesAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadDailySummariesAsync(_selectedMonthDate.Year, _selectedMonthDate.Month);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public void ClearRefreshRequested()
        {
            _navDataService.Remove(RefreshFlagKey);
        }
    }

}
