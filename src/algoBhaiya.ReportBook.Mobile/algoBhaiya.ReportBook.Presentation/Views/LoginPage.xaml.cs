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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var users = await _repository.GetAllAsync();
        ExistingUsers.Clear();
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

        Preferences.Set("CurrentUserId", user.Id);

        await Navigation.PopAsync();

        _appNavigator.NavigateToMainShell(); // ?? Login done, show shell
    }    
}
