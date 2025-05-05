using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FilledDatesCalendarPage : ContentPage
{
    public FilledDatesCalendarPage(List<DateTime> filledDates)
    {
        InitializeComponent();
        var viewModel = new FilledDatesCalendarViewModel(filledDates, this);
        BindingContext = viewModel;

        GenerateCalendar(viewModel.DisplayMonth, viewModel.FilledDates);
    }

    private void GenerateCalendar(DateTime month, HashSet<DateTime> filledDates)
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();
        CalendarGrid.ColumnDefinitions.Clear();

        int col = 0;
        for (col = 0; col < 7; col++)
            CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        var daysOfWeek = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < 7; i++)
        {
            var label = new Label
            {
                Text = daysOfWeek[i],
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            };
            CalendarGrid.Add(label, i, 0);
        }

        DateTime firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

        int row = 1;
        col = dayOfWeek;
        CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int day = 1; day <= daysInMonth; day++)
        {
            if (col == 7)
            {
                col = 0;
                row++;
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            DateTime currentDate = new DateTime(month.Year, month.Month, day);
            bool isFilled = filledDates.Contains(currentDate.Date);

            var box = new Frame
            {
                BorderColor = isFilled ? Colors.Green : Colors.Gray,
                BackgroundColor = isFilled ? Color.FromArgb("#C8E6C9") : Colors.Transparent,
                CornerRadius = 10,
                Padding = new Thickness(5),
                Content = new Label
                {
                    Text = day.ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = isFilled ? Colors.DarkGreen : Colors.Black
                }
            };

            CalendarGrid.Add(box, col, row);
            col++;
        }
    }
}
