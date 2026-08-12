namespace algoBhaiya.ReportBook.Presentation.Views;

public enum UserDeleteChoice
{
    Cancel,
    SoftDelete,
    HardDelete
}

public partial class UserDeleteChoicePopup : ContentPage
{
    private bool _isClosing;

    public TaskCompletionSource<UserDeleteChoice> ResultSource { get; } = new();

    public string PromptText { get; }

    public UserDeleteChoicePopup(string userName)
    {
        InitializeComponent();
        PromptText = $"Choose how to remove '{userName}'.";
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.FadeTo(1, 120, Easing.CubicOut);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        Opacity = 0;
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync(UserDeleteChoice.Cancel);
        return true;
    }

    private async Task CloseAsync(UserDeleteChoice choice)
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        try
        {
            await this.FadeTo(0, 100, Easing.CubicIn);
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }

            ResultSource.TrySetResult(choice);
        }
        finally
        {
            _isClosing = false;
        }
    }

    private async void OnSoftDeleteClicked(object sender, EventArgs e) => await CloseAsync(UserDeleteChoice.SoftDelete);
    private async void OnHardDeleteClicked(object sender, EventArgs e) => await CloseAsync(UserDeleteChoice.HardDelete);
    private async void OnCancelClicked(object sender, EventArgs e) => await CloseAsync(UserDeleteChoice.Cancel);
    private async void OnBackdropTapped(object sender, TappedEventArgs e) => await CloseAsync(UserDeleteChoice.Cancel);
}
