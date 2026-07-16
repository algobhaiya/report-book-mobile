using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class MenuSheetPage : ContentPage
{
    private readonly AppShellViewModel _viewModel;

    public MenuSheetPage(AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.FadeTo(1, 120, Easing.CubicOut);
        await ((VisualElement)Content).TranslateTo(0, 0, 180, Easing.CubicOut);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        Opacity = 0;
        if (Content is VisualElement content)
        {
            content.TranslationY = 260;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.NotifyMenuClosed();
    }

    private async Task CloseAsync()
    {
        await this.FadeTo(0, 100, Easing.CubicIn);
        _viewModel.NotifyMenuClosed();
        await Shell.Current.Navigation.PopModalAsync();
    }

    private async void OnBackdropTapped(object sender, TappedEventArgs e) => await CloseAsync();

    private async void OnMonthlySummaryTapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        await _viewModel.NavigateToMonthlySummaryAsync();
    }

    private async void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        await _viewModel.NavigateToSettingsAsync();
    }

    private async void OnSwitchProfileTapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        await _viewModel.NavigateToSwitchProfileAsync();
    }

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        await CloseAsync();
        await _viewModel.LogoutAsync();
    }
}
