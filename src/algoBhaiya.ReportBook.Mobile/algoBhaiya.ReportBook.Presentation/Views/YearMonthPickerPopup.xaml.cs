using System.Globalization;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class YearMonthPickerPopup : ContentPage
{
    public int SelectedYear { get; private set; }
    public int SelectedMonth { get; private set; }

    public TaskCompletionSource<(int Year, int Month)?> ResultSource { get; } = new();

    public YearMonthPickerPopup()
    {
        InitializeComponent();

        var thisYear = DateTime.Now.Year;
        for (int i = thisYear - 5; i <= thisYear + 1; i++)
            YearPicker.Items.Add(i.ToString());

        for (int m = 1; m <= 12; m++)
            MonthPicker.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m));
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        if (YearPicker.SelectedIndex == -1 || MonthPicker.SelectedIndex == -1)
        {
            ResultSource.SetResult(null); return;
        }

        SelectedYear = int.Parse(YearPicker.Items[YearPicker.SelectedIndex]);
        SelectedMonth = MonthPicker.SelectedIndex + 1;

        ResultSource.SetResult((SelectedYear, SelectedMonth));
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ResultSource.SetResult(null);
    }
}
