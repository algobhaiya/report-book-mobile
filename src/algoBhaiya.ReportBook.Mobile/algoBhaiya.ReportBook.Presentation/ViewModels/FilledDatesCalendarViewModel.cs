using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class FilledDatesCalendarViewModel : BaseViewModel
    {
        public DateTime DisplayMonth { get; }
        public string DisplayMonthTitle => DisplayMonth.ToString("MMMM yyyy");
        public HashSet<DateTime> FilledDates { get; }

        public ICommand CloseCommand { get; }

        private readonly Page _page;

        public FilledDatesCalendarViewModel(List<DateTime> filledDates, DateTime firstMonthDate, Page page)
        {
            _page = page;
            DisplayMonth = firstMonthDate;
            FilledDates = filledDates.Select(d => d.Date).ToHashSet();

            CloseCommand = new Command(async () => await _page.Navigation.PopModalAsync());
        }
    }

}
