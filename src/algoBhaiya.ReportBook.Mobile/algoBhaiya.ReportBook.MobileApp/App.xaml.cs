using algoBhaiya.ReportBook.Core.Interfaces;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppNavigator _navigator;

        public App(
            IServiceProvider serviceProvider,
            IAppNavigator navigator)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _navigator = navigator;
            
            NavigateToUserPage();

        }

        private void NavigateToUserPage()
        {
            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
                _navigator.NavigateToMainShell();
            else
                _navigator.NavigateToLogin();
        }
    }

}
