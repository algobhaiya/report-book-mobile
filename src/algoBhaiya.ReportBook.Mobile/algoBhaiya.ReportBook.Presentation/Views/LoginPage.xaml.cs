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
    private bool _isLoading;
    private bool _hasExistingUsers;
    private bool _isDeleteFlowActive;

    public ObservableCollection<AppUser> ExistingUsers { get; set; } = new ();
    public Command<AppUser> UserTappedCommand { get; }
    public Command<AppUser> RemoveUserCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool HasExistingUsers
    {
        get => _hasExistingUsers;
        set
        {
            if (_hasExistingUsers == value)
                return;

            _hasExistingUsers = value;
            OnPropertyChanged();
        }
    }

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
                await WaitForStartupInitializationAsync();
                Preferences.Set(Constants.Constants.AppUser.CurrentUserId, selectedUser.Id);
                _appNavigator.NavigateToMainShell();
            }
        });

        RemoveUserCommand = new Command<AppUser>(async (user) => await OnRemoveUserClicked(user));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Yield();

        if (_isDeleteFlowActive)
        {
            return;
        }

        await RefreshUsersAsync();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await WaitForStartupInitializationAsync();

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

        Preferences.Set(Constants.Constants.AppUser.CurrentUserId, user.Id);
        _appNavigator.NavigateToMainShell();
    }

    private async Task OnRemoveUserClicked(AppUser user)
    {
        if (_isDeleteFlowActive)
        {
            return;
        }

        _isDeleteFlowActive = true;
        try
        {
            var popup = new UserDeleteChoicePopup(user.UserName);
            await Navigation.PushModalAsync(popup);

            var choice = await popup.ResultSource.Task;

            switch (choice)
            {
                case UserDeleteChoice.SoftDelete:
                    user.IsDeleted = true;
                    await _repository.UpdateAsync(user);
                    HandleDeletedCurrentUserAsync(user);
                    RemoveUserFromList(user);
                    break;

                case UserDeleteChoice.HardDelete:
                    await DeleteUserPermanentlyAsync(user);
                    HandleDeletedCurrentUserAsync(user);
                    RemoveUserFromList(user);
                    break;
            }
        }
        finally
        {
            _isDeleteFlowActive = false;
        }
    }

    private static Task WaitForStartupInitializationAsync()
    {
        return (Application.Current as IStartupInitializationService)?.StartupInitializationTask
               ?? Task.CompletedTask;
    }

    private async Task RefreshUsersAsync()
    {
        try
        {
            IsLoading = true;
            ExistingUsers.Clear();

            var users = await _repository.GetListAsync(u => !u.IsDeleted);
            foreach (var user in users)
                ExistingUsers.Add(user);

            HasExistingUsers = ExistingUsers.Count > 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void HandleDeletedCurrentUserAsync(AppUser deletedUser)
    {
        var currentUserId = Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
        if (currentUserId != deletedUser.Id)
        {
            return;
        }

        Preferences.Set(Constants.Constants.AppUser.CurrentUserId, 0);
        _appNavigator.NavigateToLogin();
    }

    private void RemoveUserFromList(AppUser user)
    {
        var existingUser = ExistingUsers.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            ExistingUsers.Remove(existingUser);
        }

        HasExistingUsers = ExistingUsers.Count > 0;
    }

    private async Task DeleteUserPermanentlyAsync(AppUser user)
    {
        var confirm = await DisplayAlert("Sure!", $"Delete '{user.UserName}' Permanently?", "Yes", "No");
        if (confirm)
        {
            var dailyRepo = _serviceProvider.GetRequiredService<IRepository<DailyEntry>>();
            var planRepo = _serviceProvider.GetRequiredService<IRepository<MonthlyTarget>>();
            var fieldRepo = _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>();

            var dailyReportsTask = dailyRepo.GetListAsync(d => d.UserId == user.Id);
            var plansTask = planRepo.GetListAsync(d => d.UserId == user.Id);
            var fieldsTask = fieldRepo.GetListAsync(d => d.UserId == user.Id);

            await Task.WhenAll(dailyReportsTask, plansTask, fieldsTask);

            var dailyReports = await dailyReportsTask;
            var plans = await plansTask;
            var fields = await fieldsTask;

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
