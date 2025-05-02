
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Views;

namespace algoBhaiya.ReportBook.MobileApp.Services
{
    public class AppNavigator : IAppNavigator
    {
        private readonly IServiceProvider _sp;

        public AppNavigator(IServiceProvider sp)
        {
            _sp = sp;
        }

        public void NavigateToMainShell()
        {
            var shell = _sp.GetRequiredService<AppShell>();            
            Application.Current.MainPage = shell;
        }

        public void NavigateToLogin()
        {
            var loginPage = _sp.GetRequiredService<LoginPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }
    }
}
