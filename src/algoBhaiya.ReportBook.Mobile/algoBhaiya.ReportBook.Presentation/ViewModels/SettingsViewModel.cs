using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public List<int> ModificationDurationOptions { get; } = new() { 7, 15, 20, 30 };
        public List<int> DataRemovalPeriodOptions { get; } = new() { 1, 3, 6, 12, 18, 24, 30, 36 };

        private int _selectedModificationDuration;
        public int SelectedModificationDuration
        {
            get => _selectedModificationDuration;
            set => SetProperty(ref _selectedModificationDuration, value);
        }

        private int _selectedDataRemovalPeriod;
        public int SelectedDataRemovalPeriod
        {
            get => _selectedDataRemovalPeriod;
            set => SetProperty(ref _selectedDataRemovalPeriod, value);
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            // Load from Preferences or a config service
            SelectedModificationDuration = Preferences.Get(Constants.Constants.Setting.ModificationDuration, 15);
            SelectedDataRemovalPeriod = Preferences.Get(Constants.Constants.Setting.DataRemovalPeriod, 6);

            SaveSettingsCommand = new Command(SaveSettings);
        }

        private async void SaveSettings()
        {
            Preferences.Set(Constants.Constants.Setting.ModificationDuration, SelectedModificationDuration);
            Preferences.Set(Constants.Constants.Setting.DataRemovalPeriod, SelectedDataRemovalPeriod);

            await Shell.Current.DisplayAlert("Success", "Settings updated!", "OK");

            await Shell.Current.Navigation.PopAsync();
        }
    }

}
