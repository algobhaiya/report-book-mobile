using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using algoBhaiya.ReportBooks.Core.Interfaces;
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

        private bool _isNavigating = false;
        public ICommand OpenEntryCommand { get; }

        public DailyEntryListViewModel(
            IDailyEntryRepository repository,
            IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            OpenEntryCommand = new Command<DailyEntrySummaryViewModel>(async (selectedItem) =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

                try
                {
                    _isNavigating = true;

                    Preferences.Set("CurrentUserId", 1);

                    var dailyEntryViewModel = _serviceProvider.GetRequiredService<DailyEntryViewModel>();
                    dailyEntryViewModel.LoadingDateTime = selectedItem.Date;

                    var dailyEntryPage = _serviceProvider.GetRequiredService<DailyEntryPage>();
                    await Shell.Current.Navigation.PushAsync(dailyEntryPage);
                }
                finally
                {
                    _isNavigating = false;
                }
            });

            LoadDailySummariesAsync(DateTime.Today.Year, DateTime.Today.Month);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private async void LoadDailySummariesAsync(int year, int month)
        {
            DailySummaries.Clear();
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) return;

            CurrentMonthLabel = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
            var fieldTemplateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var templates = await fieldTemplateRepo.GetAllAsync();
            var totalFieldCount = templates.Count();

            var entries = await _repository.GetMonthlyEntrySummaryAsync(userId, year, month);
            
            foreach (var item in entries)
            {
                DailySummaries.Add(new DailyEntrySummaryViewModel
                {
                    Date = item.Date,
                    DateString = item.Date.ToString("dd MMMM yyyy"),
                    FilledCount = item.FilledCount,
                    TotalCount = item.TotalFields,
                    StatusText = item.FilledCount > 0 ? $"Filled: {item.FilledCount}/{totalFieldCount}" : $"Pending: 0/{totalFieldCount}",
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
            LoadDailySummariesAsync(year, month);            
        }
    }

}
