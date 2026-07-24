using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class MonthlySummaryViewModel : BaseViewModel
    {
        private readonly IAppNavigator _appNavigator;
        private readonly IServiceProvider _serviceProvider;
        private bool _isLoadingMonth;

        public ObservableCollection<MonthlySummaryItem> MonthlySummaries { get; } = new();

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
        public bool ShowEmptyState => HasLoadedMonth && !IsLoadingMonth && MonthlySummaries.Count == 0;

        private bool _isNavigating = false;
        public ICommand ShowCalendarCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public MonthlySummaryViewModel(
            IAppNavigator appNavigator, 
            IServiceProvider serviceProvider)
        {
            _appNavigator = appNavigator;
            _serviceProvider = serviceProvider;

            ShowCalendarCommand = new Command<MonthlySummaryItem>(async (selectedItem) =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

                try
                {
                    _isNavigating = true;

                    await ShowCalendar(selectedItem);
                }
                finally
                {
                    _isNavigating = false;
                }
            });

            PreviousMonthCommand = new Command(async () => await NavigateMonthAsync(-1), () => !IsLoadingMonth);
            NextMonthCommand = new Command(async () => await NavigateMonthAsync(1), () => !IsLoadingMonth);
        }

        public bool IsLoadingMonth
        {
            get => _isLoadingMonth;
            private set
            {
                if (SetProperty(ref _isLoadingMonth, value))
                {
                    OnPropertyChanged(nameof(ShowEmptyState));
                    (PreviousMonthCommand as Command)?.ChangeCanExecute();
                    (NextMonthCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public async Task LoadDataAsync(int year, int month)
        {
            if (IsLoadingMonth)
            {
                return;
            }

            IsLoadingMonth = true;
            try
            {
                _hasLoadedMonth = true;
                OnPropertyChanged(nameof(ShowEmptyState));
                MonthlySummaries.Clear();

                byte userId = (byte)Preferences.Get("CurrentUserId", 0);
                if (userId == 0) return;

                var records = await _serviceProvider
                    .GetRequiredService<IDailyEntryRepository>()
                    .GetMonthlySummaryReportAsync(userId, year, month);

                foreach (var record in records)
                {
                    MonthlySummaries.Add(record);
                }

                _currentMonthDate = new DateTime(year, month, 1);
                CurrentMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
            }
            finally
            {
                IsLoadingMonth = false;
            }
        }

        private async Task ShowCalendar(MonthlySummaryItem item)
        {
            await _appNavigator.PushModalAsync(() =>
            {
                var page = new FilledDatesCalendarPage(item.FilledDates, _currentMonthDate);
                return page;
            });
        }

        private async Task NavigateMonthAsync(int offset)
        {
            if (_currentMonthDate == default || IsLoadingMonth)
            {
                return;
            }

            var targetMonth = _currentMonthDate.AddMonths(offset);
            await LoadDataAsync(targetMonth.Year, targetMonth.Month);
        }
    }

}
