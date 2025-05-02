
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class AppShellViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _appNavigator;

        public ICommand OpenMenuCommand { get; }

        public AppShellViewModel(
            IServiceProvider serviceProvider,
            IAppNavigator appNavigator)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;

            OpenMenuCommand = new Command(OpenMenu);
        }
       
        private async void OpenMenu()
        {
            var page = Shell.Current?.CurrentPage;

            if (page == null) return;

            string action = await page.DisplayActionSheet("Menu", "Cancel", null,
                "Monthly Summary", "Settings", "Profiles", "Logout");

            switch (action)
            {
                case "Monthly Summary":
                    await _appNavigator.NavigateToAsync<MonthlyTargetPage>();
                    break;
                case "Settings":
                    await _appNavigator.NavigateToAsync<LoginPage>();
                    break;
                case "Profiles":
                    await _appNavigator.NavigateToAsync<LoginPage>();
                    break;
                case "Logout":
                    // Optional: Confirm logout first
                    bool confirm = await page.DisplayAlert("Confirm", "Logout?", "Yes", "No");
                    if (confirm)
                    {
                        // Handle logout logic
                        Preferences.Set("CurrentUserId", 0);

                        _appNavigator.NavigateToLogin();
                    }
                    break;
            }
        }

    }
}

