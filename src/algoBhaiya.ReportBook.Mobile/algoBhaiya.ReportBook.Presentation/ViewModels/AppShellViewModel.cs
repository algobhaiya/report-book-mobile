
using algoBhaiya.ReportBook.Presentation.Views;
using System.Windows.Input;

namespace algoBhaiya.ReportBook.Presentation.ViewModels
{
    public class AppShellViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        public ICommand OpenMenuCommand { get; }

        public AppShellViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            OpenMenuCommand = new Command(OpenMenu);
        }

        private async void OpenMenu()
        {
            // Resolve logInPage with logInPage from the DI container
            var templatePage = _serviceProvider.GetRequiredService<LoginPage>();

            // Navigate to logInPage
            await Shell.Current.Navigation.PushAsync(templatePage);
        }
    }
}

