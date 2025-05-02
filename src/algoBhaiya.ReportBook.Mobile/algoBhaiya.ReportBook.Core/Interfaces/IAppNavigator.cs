
namespace algoBhaiya.ReportBook.Core.Interfaces
{
    public interface IAppNavigator
    {
        void NavigateToMainShell();
        void NavigateToLogin();
        Task NavigateToAsync<TPage>();
    }
}
