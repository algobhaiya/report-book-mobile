
using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.ComponentModel;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class AppShellViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _appNavigator;

        public ICommand OpenMenuCommand { get; }

        private string _loggedInUserName;
        public string LoggedInUserName
        {
            get => _loggedInUserName;
            set
            {
                if (_loggedInUserName != value)
                {
                    _loggedInUserName = value;
                    OnPropertyChanged(nameof(LoggedInUserName));
                }
            }
        }

        public string PageTitle => "Daily Report";

        public AppShellViewModel(
            IServiceProvider serviceProvider,
            IAppNavigator appNavigator)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;

            OpenMenuCommand = new Command(OpenMenu);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void OpenMenu()
        {
            var page = Shell.Current?.CurrentPage;

            if (page == null) return;

            string action = await page.DisplayActionSheet("Menu", "Cancel", null,
                "Monthly Summary", "Settings", "Change Profile", "Logout");

            switch (action)
            {
                case "Monthly Summary":
                    await _appNavigator.NavigateToAsync<MonthlyTargetPage>();
                    break;
                case "Settings":
                    await _appNavigator.NavigateToAsync<LoginPage>();
                    break;
                case "Change Profile":
                    await _appNavigator.NavigateToSwitchProfileAsync();
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

        public async Task LoadUserNameAsync()
        {
            byte loggedInUserId = (byte)Preferences.Get("CurrentUserId", 0);

            LoggedInUserName = (await _serviceProvider
                .GetRequiredService<IRepository<AppUser>>()
                .GetFirstOrDefaultAsync(u => u.Id == loggedInUserId)).UserName;
        }
    }
}

