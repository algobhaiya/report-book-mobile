using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class LoginPage : ContentPage
{
    private readonly IRepository<AppUser> _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppNavigator _appNavigator;

    public ObservableCollection<AppUser> ExistingUsers { get; set; } = new ();
    public Command<AppUser> UserTappedCommand { get; }
    public Command<AppUser> RemoveUserCommand { get; }

    public LoginPage(
        IRepository<AppUser> repository,
        IServiceProvider serviceProvider,
        IAppNavigator appNavigator
        )
    {
        InitializeComponent();
        _repository = repository;
        _serviceProvider = serviceProvider;
        _appNavigator = appNavigator;
        BindingContext = this;

        UserTappedCommand = new Command<AppUser>(async (selectedUser) =>
        {
            if (selectedUser != null)
            {
                Preferences.Set("CurrentUserId", selectedUser.Id);

                await Navigation.PopAsync();

                _appNavigator.NavigateToMainShell();
            }
        });

        RemoveUserCommand = new Command<AppUser>(async (user) => await OnRemoveUserClicked(user));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ExistingUsers.Clear();

        var users = await _repository.GetListAsync(u => !u.IsDeleted);        
        foreach (var user in users)
            ExistingUsers.Add(user);
    }

    // Login or register using input field
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            await DisplayAlert("Error", "Please enter a valid username.", "OK");
            return;
        }

        var existingUser = (await _repository.GetAllAsync())
            .Where(u => u.UserName == username)
            .FirstOrDefault();

        AppUser user;

        if (existingUser != null)
        {
            user = existingUser;
        }
        else
        {
            user = new AppUser { UserName = username };
            await _repository.AddAsync(user);
        }

        if (user.IsDeleted)
        {
            user.IsDeleted = false;
            await _repository.UpdateAsync(user);
        }

        Preferences.Set("CurrentUserId", user.Id);

        await Navigation.PopAsync();

        _appNavigator.NavigateToMainShell(); // ?? Login done, show shell
    }

    private async Task OnRemoveUserClicked(AppUser user)
    {
        string action = await DisplayActionSheet(
            $"Remove user '{user.UserName}'?",
            "Cancel",
            null,
            "Hide User (Soft Delete)",
            "Delete Permanently");

        switch (action)
        {
            case "Hide User (Soft Delete)":
                user.IsDeleted = true; // You need to add this flag in your AppUser entity
                await _repository.UpdateAsync(user);
                break;

            case "Delete Permanently":
                await DeleteUserPermanentlyAsync(user);
                break;
        }

        // Refresh the user list        
        ExistingUsers.Clear();

        var users = await _repository.GetListAsync(u => !u.IsDeleted);
        foreach (var u in users)
            ExistingUsers.Add(u);
    }

    private async Task DeleteUserPermanentlyAsync(AppUser user)
    {
        var confirm = await DisplayAlert("Sure!", $"Delete '{user.UserName}' Permanently?", "Yes", "No");
        if (confirm)
        {
            /*
             delete daily items
             delete monthly plans
             delete Field templates
             delete users
             */

            var dailyRepo = _serviceProvider.GetRequiredService<IRepository<DailyEntry>>();
            var planRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var fieldRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var dailyReportsTask = dailyRepo.GetListAsync(d => d.UserId == user.Id);
            var plansTask = planRepo.GetListAsync(d => d.UserId == user.Id);
            var fieldsTask = fieldRepo.GetListAsync(d => d.UserId == user.Id);

            await Task.WhenAll(dailyReportsTask, plansTask, fieldsTask);

            var dailyReports = dailyReportsTask.Result;
            var plans = plansTask.Result;
            var fields = fieldsTask.Result;

            foreach (var d in dailyReports)
            {
                await dailyRepo.DeleteAsync(d);
            }

            foreach (var p in plans)
            {
                await planRepo.DeleteAsync(p);
            }

            foreach (var f in fields)
            {
                await fieldRepo.DeleteAsync(f);
            }

            await _repository.DeleteAsync(user);
        }
    }
}
