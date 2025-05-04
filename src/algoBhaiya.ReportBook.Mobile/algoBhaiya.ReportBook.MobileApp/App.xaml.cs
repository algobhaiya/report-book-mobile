using algoBhaiya.ReportBook.Core.Interfaces;

namespace algoBhaiya.ReportBook.MobileApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(
            IServiceProvider serviceProvider,
            IAppNavigator navigator)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            int currentUserId = Preferences.Get("CurrentUserId", 0);

            if (currentUserId > 0)
                navigator.NavigateToMainShell();
            else
                navigator.NavigateToLogin();
        }
    }

}
