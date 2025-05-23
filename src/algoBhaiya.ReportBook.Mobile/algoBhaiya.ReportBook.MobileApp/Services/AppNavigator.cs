﻿
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

        public async Task NavigateToAsync<TPage>()
        {
            var route = typeof(TPage).Name;
            await Shell.Current.GoToAsync(route);
        }

        public async Task NavigateToSwitchProfileAsync()
        {
            var page = _sp.GetRequiredService<SwitchProfilePage>();
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        public async Task PopModalAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        public async Task PushModalAsync(Func<object> pageFactory)
        {
            if (pageFactory.Invoke() is Page page)
            {
                await Shell.Current.Navigation.PushModalAsync(page);
            }
            else
            {
                throw new InvalidOperationException("Factory did not return a Page.");
            }
        }
    }
}
