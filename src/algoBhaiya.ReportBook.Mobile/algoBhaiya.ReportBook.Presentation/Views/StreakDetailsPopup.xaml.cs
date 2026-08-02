namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class StreakDetailsPopup : ContentPage
{
    private readonly algoBhaiya.ReportBook.Presentation.ViewModels.AppShellViewModel _viewModel;
    private bool _isClosing;

    public StreakDetailsPopup(algoBhaiya.ReportBook.Presentation.ViewModels.AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.FadeTo(1, 120, Easing.CubicOut);
        if (Content is VisualElement content)
        {
            await content.TranslateTo(0, 0, 180, Easing.CubicOut);
        }
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
        _viewModel.NotifyStreakDetailsClosed();
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
            _viewModel.NotifyStreakDetailsClosed();
            await Shell.Current.Navigation.PopModalAsync();
            return true;
        }
        finally
        {
            _isClosing = false;
        }
    }

    private async void OnBackdropTapped(object sender, TappedEventArgs e) => await TryCloseAsync();

}
