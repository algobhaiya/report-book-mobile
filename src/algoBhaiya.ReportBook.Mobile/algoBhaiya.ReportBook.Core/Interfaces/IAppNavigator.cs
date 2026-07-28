
namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IAppNavigator
    {
        void NavigateToMainShell();
        void NavigateToLogin();
        void NavigateToPlanner();
        Task NavigateToAsync<TPage>();
        Task NavigateToSwitchProfileAsync();
        Task PopModalAsync();
        Task PushModalAsync(Func<object> pageFactory); // object is the Page.
    }
}
