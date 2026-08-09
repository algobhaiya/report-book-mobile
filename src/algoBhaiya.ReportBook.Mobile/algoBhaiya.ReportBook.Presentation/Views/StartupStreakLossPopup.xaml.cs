namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class StartupStreakLossPopup : ContentPage
{
    private readonly algoBhaiya.ReportBook.Presentation.ViewModels.AppShellViewModel _viewModel;
    private bool _isClosing;

    public StartupStreakLossPopup(algoBhaiya.ReportBook.Presentation.ViewModels.AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.NotifyStartupStreakLossClosed();
    }

    private async Task<bool> TryCloseAsync()
    {
        if (_isClosing)
        {
            return false;
        }

        _isClosing = true;
        try
        {
            await this.FadeTo(0, 100, Easing.CubicIn);
            _viewModel.NotifyStartupStreakLossClosed();
            await Shell.Current.Navigation.PopModalAsync();
            return true;
        }
        finally
        {
            _isClosing = false;
        }
    }

    private async void OnDismissClicked(object sender, EventArgs e) => await TryCloseAsync();
    private async void OnBackdropTapped(object sender, TappedEventArgs e) => await TryCloseAsync();
}
