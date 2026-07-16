
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
        private bool _isMenuOpen;

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

            OpenMenuCommand = new Command(async () => await OpenMenuAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async Task OpenMenuAsync()
        {
            if (_isMenuOpen)
            {
                return;
            }

            _isMenuOpen = true;

            try
            {
                await _appNavigator.PushModalAsync(() =>
                    new MenuSheetPage(this));
            }
            catch
            {
                _isMenuOpen = false;
                throw;
            }
        }

        public void NotifyMenuClosed()
        {
            _isMenuOpen = false;
        }

        public async Task NavigateToMonthlySummaryAsync()
        {
            await _appNavigator.NavigateToAsync<MonthlySummaryPage>();
        }

        public async Task NavigateToSettingsAsync()
        {
            await _appNavigator.NavigateToAsync<SettingsPage>();
        }

        public async Task NavigateToSwitchProfileAsync()
        {
            await _appNavigator.PushModalAsync(() =>
                _serviceProvider.GetRequiredService<SwitchProfilePage>());
        }

        public async Task LogoutAsync()
        {
            var page = Shell.Current?.CurrentPage;
            if (page == null)
            {
                return;
            }

            bool confirm = await page.DisplayAlert("Logout", "Do you want to log out now?", "Yes", "No");
            if (!confirm)
            {
                return;
            }

            Preferences.Set("CurrentUserId", 0);
            _appNavigator.NavigateToLogin();
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

