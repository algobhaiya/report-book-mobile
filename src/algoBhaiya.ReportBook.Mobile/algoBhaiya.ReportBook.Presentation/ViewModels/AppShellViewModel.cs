
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
        private readonly IPlannerCatalogService _plannerCatalogService;
        private readonly ITrackingStreakService _trackingStreakService;
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

        private string _pageTitle = "Daily Report";
        public string PageTitle
        {
            get => _pageTitle;
            private set
            {
                if (_pageTitle != value)
                {
                    _pageTitle = value;
                    OnPropertyChanged(nameof(PageTitle));
                }
            }
        }

        private int _streakCount;
        public int StreakCount
        {
            get => _streakCount;
            private set
            {
                if (_streakCount != value)
                {
                    _streakCount = value;
                    OnPropertyChanged(nameof(StreakCount));
                    OnPropertyChanged(nameof(StreakText));
                }
            }
        }

        public string StreakText => StreakCount.ToString();

        public AppShellViewModel(
            IServiceProvider serviceProvider,
            IAppNavigator appNavigator,
            IPlannerCatalogService plannerCatalogService,
            ITrackingStreakService trackingStreakService)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;
            _plannerCatalogService = plannerCatalogService;
            _trackingStreakService = trackingStreakService;

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

        public void UpdatePageTitle(string? title)
        {
            PageTitle = string.IsNullOrWhiteSpace(title) ? "Daily Report" : title.Trim();
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
            if (loggedInUserId == 0)
            {
                LoggedInUserName = string.Empty;
                StreakCount = 0;
                return;
            }

            var user = await _serviceProvider
                .GetRequiredService<IRepository<AppUser>>()
                .GetFirstOrDefaultAsync(u => u.Id == loggedInUserId);

            LoggedInUserName = user?.UserName ?? string.Empty;
            await RefreshStreakAsync(loggedInUserId);

            if (Preferences.Get(Constants.Constants.AppState.PlannerBypassGateKey, false))
            {
                Preferences.Set(Constants.Constants.AppState.PlannerBypassGateKey, false);
                return;
            }

            var hasActiveFields = await _plannerCatalogService.HasActiveFieldsAsync(loggedInUserId);
            if (!hasActiveFields)
            {
                _appNavigator.NavigateToPlanner();
            }
        }

        public async Task RefreshStreakAsync()
        {
            byte loggedInUserId = (byte)Preferences.Get("CurrentUserId", 0);
            await RefreshStreakAsync(loggedInUserId);
        }

        public async Task RefreshStreakAsync(byte userId)
        {
            if (userId == 0)
            {
                StreakCount = 0;
                return;
            }

            StreakCount = await _trackingStreakService.GetCurrentStreakAsync(userId);
        }
    }
}

