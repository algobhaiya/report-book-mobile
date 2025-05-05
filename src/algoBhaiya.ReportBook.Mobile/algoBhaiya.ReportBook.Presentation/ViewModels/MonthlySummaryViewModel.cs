using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class MonthlySummaryViewModel : BaseViewModel
    {
        private readonly IAppNavigator _appNavigator;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<MonthlySummaryItem> MonthlySummaries { get; } = new();

        private bool _isNavigating = false;
        public ICommand ShowCalendarCommand { get; }

        public MonthlySummaryViewModel(
            IAppNavigator appNavigator, 
            IServiceProvider serviceProvider)
        {
            _appNavigator = appNavigator;
            
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

            LoadData();
            _serviceProvider = serviceProvider;
        }

        private void LoadData()
        {
            // Populate with your real data
            for (int i = 0; i < 30; i++)
            {
                MonthlySummaries.Add(new MonthlySummaryItem
                {
                    ItemName = "Work Hours",
                    TotalDays = 20,
                    AverageValue = 7.5,
                    TotalSum = 150,
                    Target = 180,
                    FilledDates = new List<DateTime> { /* filled dates */ }
                });
            }

            // Add more items...
        }

        private async Task ShowCalendar(MonthlySummaryItem item)
        {
            await _appNavigator.PushModalAsync(() =>
            {
                var page = new FilledDatesCalendarPage(
    new List<DateTime> { DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-2), DateTime.Today });
                //var page = new FilledDatesCalendarPage(item.FilledDates);
                return page;
            });
        }
    }

}
