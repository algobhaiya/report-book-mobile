using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class SwitchProfilePageViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _appNavigator;

        public ObservableCollection<AppUser> Profiles { get; } = new();
        
        private bool _isNavigating = false;
        public ICommand SelectUserCommand { get; }

        public SwitchProfilePageViewModel(
            IServiceProvider serviceProvider,
            IAppNavigator appNavigator)
        {
            _serviceProvider = serviceProvider;
            _appNavigator = appNavigator;

            SelectUserCommand = new Command<AppUser>(async (selectedItem) =>
            {
                if (selectedItem == null || _isNavigating)
                    return;

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

            LoadProfiles();            
        }

        private async void LoadProfiles()
        {
            var allProfiles = await _serviceProvider
                .GetRequiredService<IRepository<AppUser>>()
                .GetListAsync(u => !u.IsDeleted);

            Profiles.Clear();
            foreach (var profile in allProfiles)
                Profiles.Add(profile);
        }

        private async Task SwitchToProfile(AppUser profile)
        {
            if (profile == null)
                return;

            Preferences.Set(Constants.Constants.AppUser.CurrentUserId, profile.Id);

            // Close modal
            await _appNavigator.PopModalAsync();

            // Go back to main shell or reload
            _appNavigator.NavigateToMainShell();
        }
    }

}
