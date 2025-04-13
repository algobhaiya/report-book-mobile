using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryListViewModel
    {
        public ObservableCollection<DailyEntrySummaryViewModel> DailySummaries { get; } = new();
        
        private readonly IDailyEntryRepository _repository;
        private readonly IServiceProvider _serviceProvider;
        
        public string CurrentMonthLabel => DateTime.Today.ToString("MMMM yyyy");

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

            LoadDailySummaries();
        }
        private async void LoadDailySummaries()
        {
            DailySummaries.Clear();
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) return;

            var fieldTemplateRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var templates = await fieldTemplateRepo.GetAllAsync();
            var totalFieldCount = templates.Count();

            var entries = await _repository.GetMonthlyEntrySummaryAsync(userId, DateTime.Today.Year, DateTime.Today.Month);
            
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
    }

}
