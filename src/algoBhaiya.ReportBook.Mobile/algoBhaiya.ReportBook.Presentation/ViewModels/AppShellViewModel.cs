
using algoBhaiya.ReportBook.Core.Dtos;
using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
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
        private bool _isStreakDetailsOpen;
        private bool _isStartupStreakLossOpen;
        private readonly ObservableCollection<StreakWeekDayViewModel> _weeklyDays = new();

        public ICommand OpenMenuCommand { get; }
        public ICommand OpenStreakDetailsCommand { get; }
        public IReadOnlyList<StreakWeekDayViewModel> WeeklyDays => _weeklyDays;

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
                    OnPropertyChanged(nameof(CurrentStreakDisplay));
                    OnPropertyChanged(nameof(NextMilestoneDisplay));
                    OnPropertyChanged(nameof(MilestoneProgress));
                }
            }
        }

        public string StreakText => StreakCount.ToString();
        public string CurrentStreakDisplay => $"{StreakCount} Days";
        public string NextMilestoneDisplay => $"{GetNextMilestone(StreakCount)} Days";
        public double MilestoneProgress => GetMilestoneProgress(StreakCount);

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
            OpenStreakDetailsCommand = new Command(async () => await OpenStreakDetailsAsync());
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

        private async Task OpenStreakDetailsAsync()
        {
            if (_isStreakDetailsOpen)
            {
                return;
            }

            _isStreakDetailsOpen = true;

            try
            {
                await RefreshWeeklyDaysAsync();
                await RefreshStreakAsync();
                await _appNavigator.PushModalAsync(() =>
                    new StreakDetailsPopup(this));
            }
            catch
            {
                _isStreakDetailsOpen = false;
                throw;
            }
        }

        public void NotifyStreakDetailsClosed()
        {
            _isStreakDetailsOpen = false;
        }

        public async Task ShowStartupStreakLossAsync()
        {
            if (_isStartupStreakLossOpen)
            {
                return;
            }

            byte userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            if (userId == 0)
            {
                return;
            }

            _isStartupStreakLossOpen = true;
            try
            {
                await _appNavigator.PushModalAsync(() => new StartupStreakLossPopup(this));
            }
            catch
            {
                _isStartupStreakLossOpen = false;                
            }
        }

        public async Task<StreakRefreshResult> RefreshStartupStreakAsync()
        {
            byte userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            if (userId == 0)
            {
                StreakCount = 0;
                return new StreakRefreshResult();
            }

            var refreshResult = await _trackingStreakService.RefreshStreakForStartupAsync(userId);
            StreakCount = refreshResult.StreakCount;
            return refreshResult;
        }

        public void NotifyStartupStreakLossClosed()
        {
            _isStartupStreakLossOpen = false;
        }

        public void UpdatePageTitle(string? title)
        {
            PageTitle = string.IsNullOrWhiteSpace(title) ? "Daily Report" : title.Trim();
        }

        public async Task RefreshWeeklyDaysAsync()
        {
            byte userId = (byte)Preferences.Get("CurrentUserId", 0);
            await RefreshWeeklyDaysAsync(userId);
        }

        private async Task RefreshWeeklyDaysAsync(byte userId)
        {
            _weeklyDays.Clear();
            OnPropertyChanged(nameof(WeeklyDays));

            if (userId == 0)
            {
                BuildEmptyWeeklyDays(DateTime.Today);
                return;
            }

            var today = DateTime.Today.Date;
            var weekStart = today.AddDays(-6);
            var weekEnd = today;

            var entries = await _serviceProvider
                .GetRequiredService<IDailyEntryRepository>()
                .GetEntriesForUserThroughDateAsync(userId, weekEnd);

            var countsByDate = entries
                .GroupBy(x => x.Date.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var items = new List<StreakWeekDayViewModel>();
            for (var offset = 0; offset < 7; offset++)
            {
                var date = weekStart.AddDays(offset);
                countsByDate.TryGetValue(date, out var filledCount);
                items.Add(new StreakWeekDayViewModel
                {                    
                    DayLabel = GetDayLabel(date),
                    FilledCount = filledCount,
                    IsToday = date == today
                });
            }

            var max = Math.Max(items.Count == 0 ? 0 : items.Max(x => x.FilledCount), 1);
            var hasAnyFilledEntries = items.Any(x => x.FilledCount > 0);
            foreach (var item in items)
            {
                item.BarProgress = item.FilledCount == 0 ? 0.12 : Math.Clamp((double)item.FilledCount / max, 0.12, 1);
                item.BarHeight = hasAnyFilledEntries
                    ? 18 + (92 * item.BarProgress)
                    : 16 + (32 * item.BarProgress);
                item.IsEmpty = item.FilledCount == 0;
                _weeklyDays.Add(item);
            }

            OnPropertyChanged(nameof(WeeklyDays));
        }

        private void BuildEmptyWeeklyDays(DateTime today)
        {
            var weekStart = today.AddDays(-6);
            for (var offset = 0; offset < 7; offset++)
            {
                var date = weekStart.AddDays(offset);
                _weeklyDays.Add(new StreakWeekDayViewModel
                {
                    DayLabel = GetDayLabel(date),
                    FilledCount = 0,
                    BarProgress = 0.12,
                    BarHeight = 20,
                    IsToday = date == today,
                    IsEmpty = true
                });
            }

            OnPropertyChanged(nameof(WeeklyDays));
        }

        private static string GetDayLabel(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => "Sat",
                DayOfWeek.Sunday => "Sun",
                DayOfWeek.Monday => "Mon",
                DayOfWeek.Tuesday => "Tue",
                DayOfWeek.Wednesday => "Wed",
                DayOfWeek.Thursday => "Thu",
                DayOfWeek.Friday => "Fri",
                _ => string.Empty,
            };
        }

        private static int GetNextMilestone(int currentStreak)
        {
            int[] milestones = [7, 14, 21, 30, 60, 90, 120, 180, 250, 365, 730, 1000];
            foreach (var milestone in milestones)
            {
                if (currentStreak < milestone)
                {
                    return milestone;
                }
            }

            return milestones[^1];
        }

        private static double GetMilestoneProgress(int currentStreak)
        {
            var milestone = GetNextMilestone(currentStreak);
            return milestone <= 0 ? 0 : Math.Clamp((double)currentStreak / milestone, 0, 1);
        }

        public async Task NavigateToMonthlySummaryAsync()
        {
            await _appNavigator.NavigateToAsync<MonthlySummaryPage>();
        }

        public async Task NavigateToSettingsAsync()
        {
            await _appNavigator.NavigateToAsync<SettingsPage>();
        }

        public async Task NavigateToGuideAsync()
        {
            await _appNavigator.NavigateToAsync<HelpPage>();
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

            var userTask = _serviceProvider
                .GetRequiredService<IRepository<AppUser>>()
                .GetFirstOrDefaultAsync(u => u.Id == loggedInUserId);

            var streakTask = _trackingStreakService.GetCurrentStreakAsync(loggedInUserId);

            var hasActiveFieldsTask = Preferences.Get(Constants.Constants.AppState.PlannerBypassGateKey, false)
                ? Task.FromResult(true)
                : _plannerCatalogService.HasActiveFieldsAsync(loggedInUserId);

            await Task.WhenAll(userTask, streakTask, hasActiveFieldsTask);

            LoggedInUserName = (await userTask)?.UserName ?? string.Empty;
            StreakCount = await streakTask;

            if (Preferences.Get(Constants.Constants.AppState.PlannerBypassGateKey, false))
            {
                Preferences.Set(Constants.Constants.AppState.PlannerBypassGateKey, false);
                return;
            }

            if (!await hasActiveFieldsTask)
            {
                MainThread.BeginInvokeOnMainThread(() => _appNavigator.NavigateToPlanner());
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

