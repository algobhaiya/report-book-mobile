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

        private DateTime _selectedMonthDate = DateTime.Today;

        private bool _isNavigating = false;
        public ICommand OpenEntryCommand { get; }

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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private async Task LoadDailySummariesAsync(int year, int month)
        {
            DailySummaries.Clear();
            byte userId = (byte) Preferences.Get("CurrentUserId", 0);
            if (userId == 0) return;

            _selectedMonthDate = new DateTime(year, month, 1);

            SelectedMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
            
            var entries = await _repository.GetMonthlyEntrySummaryAsync(userId, year, month);
            
            foreach (var item in entries)
            {
                DailySummaries.Add(new DailyEntrySummaryViewModel
                {
                    Date = item.Date,
                    DateString = item.Date.ToString("dd MMMM yyyy"),
                    FilledCount = item.FilledCount,
                    TotalCount = item.TotalFields,
                    StatusText = item.FilledCount > 0 ? $"Filled: {item.FilledCount}/{item.TotalFields}" : $"Pending: 0/{item.TotalFields}",
                    StatusIcon = item.FilledCount > 0 ? "green_check_mark.svg" : "pending_gray_status.png"
                });
            }
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
            await LoadDailySummariesAsync(_selectedMonthDate.Year, _selectedMonthDate.Month);
        }
    }

}
