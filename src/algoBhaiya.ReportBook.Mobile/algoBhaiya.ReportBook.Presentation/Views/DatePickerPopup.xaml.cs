using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class DatePickerPopup : ContentPage, INotifyPropertyChanged
{
    private DateTime _displayMonth;
    private DateTime _selectedDate;
    private readonly DateTime _today = DateTime.Today;

    public TaskCompletionSource<DateTime?> ResultSource { get; } = new();

    public string MonthTitle => _displayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    public event PropertyChangedEventHandler? PropertyChanged;

    public DatePickerPopup()
    {
        InitializeComponent();

        _selectedDate = _today;
        _displayMonth = new DateTime(_today.Year, _today.Month, 1);
        ApplyRoundedShapes();
        BindingContext = this;
        GenerateCalendar();
    }

    private void ApplyRoundedShapes()
    {
        CardBorder.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(24)
        };

        TodayChip.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(6)
        };

        SelectedChip.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(6)
        };
    }

    private void GenerateCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();
        CalendarGrid.ColumnDefinitions.Clear();

        for (var i = 0; i < 7; i++)
        {
            CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        var daysOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var i = 0; i < 7; i++)
        {
            var label = new Label
            {
                Text = daysOfWeek[i],
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Gray
            };
            CalendarGrid.Add(label, i, 0);
        }

        var firstDay = new DateTime(_displayMonth.Year, _displayMonth.Month, 1);
        var startOffset = (int)firstDay.DayOfWeek;
        var daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);

        var row = 1;
        var col = startOffset;
        CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var day = 1; day <= daysInMonth; day++)
        {
            if (col == 7)
            {
                col = 0;
                row++;
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var currentDate = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            var isSelected = currentDate.Date == _selectedDate.Date;
            var isToday = currentDate.Date == _today.Date;
            var cellBorder = isSelected
                ? Color.FromArgb("#16A34A")
                : isToday
                    ? Color.FromArgb("#2563EB")
                    : Color.FromArgb("#E2E8F0");
            var cellBackground = isSelected
                ? Color.FromArgb("#DCFCE7")
                : isToday
                    ? Color.FromArgb("#DBEAFE")
                    : Color.FromArgb("#F8FAFC");
            var cellTextColor = isSelected
                ? Color.FromArgb("#166534")
                : isToday
                    ? Color.FromArgb("#1D4ED8")
                    : Color.FromArgb("#0F172A");

            var border = new Border
            {
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(14)
                },
                Padding = new Thickness(0),
                BackgroundColor = cellBackground,
                Stroke = cellBorder,
                HeightRequest = 42,
                WidthRequest = 42,
                Content = new Label
                {
                    Text = day.ToString(),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = cellTextColor
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SelectDate(currentDate);
            border.GestureRecognizers.Add(tap);

            CalendarGrid.Add(border, col, row);
            col++;
        }
    }

    private void SelectDate(DateTime date)
    {
        _selectedDate = date.Date;
        ResultSource.TrySetResult(_selectedDate);
        GenerateCalendar();
    }

    private void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(-1);
        RaisePropertyChanged(nameof(MonthTitle));
        GenerateCalendar();
    }

    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(1);
        RaisePropertyChanged(nameof(MonthTitle));
        GenerateCalendar();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ResultSource.TrySetResult(null);
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
