using algoBhaiya.ReportBook.Presentation.Views;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnOpenDailyFormPageClicked(object sender, EventArgs e)
        {
            // Resolve dailyEntryPage with dailyEntryPage from the DI container
            var dailyEntryPage = _serviceProvider.GetRequiredService<DailyEntryPage>();

            // Navigate to dailyEntryPage
            await Navigation.PushAsync(dailyEntryPage);
        }

        private async void OnOpenLogInPageClicked(object sender, EventArgs e)
        {
            // Resolve logInPage with logInPage from the DI container
            var logInPage = _serviceProvider.GetRequiredService<LoginPage>();

            // Navigate to logInPage
            await Navigation.PushAsync(logInPage);
        }

        private async void OnOpenFieldTemplatePageClicked(object sender, EventArgs e)
        {
            // Resolve logInPage with logInPage from the DI container
            var templatePage = _serviceProvider.GetRequiredService<FieldTemplatePage>();

            // Navigate to logInPage
            await Navigation.PushAsync(templatePage);
        }

        private async void OnOpenFieldUnitPageClicked(object sender, EventArgs e)
        {
            // Resolve logInPage with logInPage from the DI container
            var unitPage = _serviceProvider.GetRequiredService<FieldUnitPage>();

            // Navigate to logInPage
            await Navigation.PushAsync(unitPage);
        }

        private async void OnOpenMonthlyTargetPageClicked(object sender, EventArgs e)
        {
            // Resolve logInPage with logInPage from the DI container
            var targetPage = _serviceProvider.GetRequiredService<MonthlyTargetPage>();

            // Navigate to logInPage
            await Navigation.PushAsync(targetPage);
        }

    }

}
