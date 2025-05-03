using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class SwitchProfilePageViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _appNavigator;

        public ObservableCollection<AppUser> Profiles { get; } = new();

        private AppUser _selectedProfile;
        public AppUser SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (SetProperty(ref _selectedProfile, value))
                    SwitchToProfile(value);
            }
        }

        public SwitchProfilePageViewModel(
            IServiceProvider serviceProvider,
            IAppNavigator appNavigator)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;

            LoadProfiles();            
        }

        private async void LoadProfiles()
        {
            var allProfiles = await _serviceProvider
                .GetRequiredService<IRepository<AppUser>>()
                .GetAllAsync();

            Profiles.Clear();
            foreach (var profile in allProfiles)
                Profiles.Add(profile);
        }

        private void SwitchToProfile(AppUser profile)
        {
            if (profile == null)
                return;

            Preferences.Set(Constants.Constants.AppUser.CurrentUserId, profile.Id);

            // Go back to main shell or reload
            _appNavigator.NavigateToMainShell();
        }
    }

}
