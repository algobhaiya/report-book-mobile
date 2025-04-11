using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class DailyEntryListViewModel
    {
        public ObservableCollection<DailyEntrySummaryViewModel> DailySummaries { get; } = new();
        public ICommand OpenEntryCommand { get; }

        private readonly IDailyEntryRepository _repository;
        private readonly IServiceProvider _serviceProvider;
        
        public string CurrentMonthLabel => DateTime.Today.ToString("MMMM yyyy");

        public DailyEntryListViewModel(
            IDailyEntryRepository repository, 
            IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            //OpenEntryCommand = new AsyncRelayCommand<DateTime>(OpenEntryAsync);

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

        private async Task OpenEntryAsync(DateTime date)
        {
            await Shell.Current.GoToAsync($"dailyentrypage?entryDate={date:yyyy-MM-dd}");
        }
    }

}
