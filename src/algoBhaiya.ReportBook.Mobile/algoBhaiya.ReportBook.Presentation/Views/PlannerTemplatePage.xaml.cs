using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Constants;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBook.Presentation.ViewModels;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class PlannerTemplatePage : ContentPage
{
    private readonly IPlannerCatalogService _catalogService;
    private readonly IAppNavigator _appNavigator;
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized;
    private bool _isLoading;
    private bool _isNavigating;

    public ObservableCollection<PlannerPresetGroup> PresetGroups { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value)
            {
                return;
            }

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public Command<PlannerPreset> OpenDetailCommand { get; }
    public Command<PlannerPreset> StartCommand { get; }

    public PlannerTemplatePage(
        IPlannerCatalogService catalogService,
        IAppNavigator appNavigator,
        IServiceProvider serviceProvider,
        NavigationDataService navDataService)
    {
        InitializeComponent();
        BindingContext = this;

        _catalogService = catalogService;
        _appNavigator = appNavigator;
        _serviceProvider = serviceProvider;
        _navDataService = navDataService;

        OpenDetailCommand = new Command<PlannerPreset>(async preset => await OpenDetailAsync(preset));
        StartCommand = new Command<PlannerPreset>(async preset => await StartPresetAsync(preset));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await LoadCatalogAsync();
            _isInitialized = true;
        }
    }

    private async Task LoadCatalogAsync()
    {
        try
        {
            IsLoading = true;
            PresetGroups.Clear();

            var presets = await _catalogService.GetCatalogAsync();
            foreach (var group in presets
                         .GroupBy(p => p.Category)
                         .OrderBy(g => g.Min(p => p.SortOrder)))
            {
                PresetGroups.Add(new PlannerPresetGroup(
                    group.Key,
                    group.OrderBy(p => p.SortOrder)));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OpenDetailAsync(PlannerPreset preset)
    {
        if (preset == null || _isLoading || _isNavigating)
        {
            return;
        }

        try
        {
            _isNavigating = true;
            _navDataService.Set(Constants.Constants.Planner.Item_ToOpen, preset);

            var detailPage = _serviceProvider.GetRequiredService<PlannerTemplateDetailPage>();
            await Navigation.PushAsync(detailPage);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task StartPresetAsync(PlannerPreset preset)
    {
        if (preset == null || _isNavigating)
        {
            return;
        }

        try
        {
            _isNavigating = true;
            IsLoading = true;

            if (preset.IsBlank)
            {
                Preferences.Set(Constants.Constants.AppState.PlannerBypassGateKey, true);
                _appNavigator.NavigateToMainShell();
                return;
            }

            var userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
            if (userId == 0)
            {
                return;
            }

            await _catalogService.StartPresetAsync(userId, preset);
            _appNavigator.NavigateToMainShell();
        }
        finally
        {
            IsLoading = false;
            _isNavigating = false;
        }
    }
}
