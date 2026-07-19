using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class SwitchProfilePageViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _appNavigator;

        public ObservableCollection<AppUser> Profiles { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                    OnPropertyChanged(nameof(ShowEmptyState));
            }
        }

        private bool _hasProfiles;
        public bool HasProfiles
        {
            get => _hasProfiles;
            private set
            {
                if (SetProperty(ref _hasProfiles, value))
                    OnPropertyChanged(nameof(ShowEmptyState));
            }
        }

        public bool ShowEmptyState => !IsLoading && !HasProfiles;

        private byte _currentUserId;
        public byte CurrentUserId
        {
            get => _currentUserId;
            private set => SetProperty(ref _currentUserId, value);
        }

        private bool _isNavigating;
        public ICommand SelectUserCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CancelCommand { get; }

        public SwitchProfilePageViewModel(IServiceProvider serviceProvider, IAppNavigator appNavigator)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;

            SelectUserCommand = new Command<AppUser>(async selectedItem =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

                if (selectedItem.Id == CurrentUserId)
                {
                    await _appNavigator.PopModalAsync();
                    return;
                }

                try
                {
                    _isNavigating = true;
                    await SwitchToProfile(selectedItem);
                }
                finally
                {
                    _isNavigating = false;
                }
            });

            RefreshCommand = new Command(async () => await LoadProfilesAsync());
            CancelCommand = new Command(async () => await _appNavigator.PopModalAsync());
        }

        public bool IsCurrentProfile(AppUser profile) =>
            profile != null && profile.Id == CurrentUserId;

        public async Task InitializeAsync()
        {
            await LoadProfilesAsync();
        }

        public async Task LoadProfilesAsync()
        {
            IsLoading = true;

            try
            {
                CurrentUserId = (byte)Preferences.Get(AppConstants.AppUser.CurrentUserId, 0);

                var allProfiles = await _serviceProvider
                    .GetRequiredService<IRepository<AppUser>>()
                    .GetListAsync(u => !u.IsDeleted);

                Profiles.Clear();
                foreach (var profile in allProfiles)
                {
                    profile.IsCurrentProfile = profile.Id == CurrentUserId;
                    Profiles.Add(profile);
                }

                HasProfiles = Profiles.Count > 0;

                if (CurrentUserId != 0 && !Profiles.Any(p => p.Id == CurrentUserId))
                {
                    CurrentUserId = 0;
                    Preferences.Set(AppConstants.AppUser.CurrentUserId, 0);
                    foreach (var profile in Profiles)
                        profile.IsCurrentProfile = false;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SwitchToProfile(AppUser profile)
        {
            Preferences.Set(AppConstants.AppUser.CurrentUserId, profile.Id);
            await _appNavigator.PopModalAsync();
            _appNavigator.NavigateToMainShell();
        }
    }
}
