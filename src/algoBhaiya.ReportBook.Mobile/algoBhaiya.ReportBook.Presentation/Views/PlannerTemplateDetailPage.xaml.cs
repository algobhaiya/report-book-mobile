using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBook.Presentation.Helpers;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class PlannerTemplateDetailPage : ContentPage
{
    private readonly IPlannerCatalogService _catalogService;
    private readonly IAppNavigator _appNavigator;
    private readonly NavigationDataService _navDataService;
    private bool _isInitialized;
    private bool _isLoading;

    private PlannerPreset _selectedPreset;
    private string _name;
    private string _category;
    private string _summary;
    private string _description;
    private string _accentHex;
    private string _isBlankText;
    private string _fieldCountText;
    private string _fieldsEmptyText;
    private bool _isBlank;
    private bool _hasFields;

    public ObservableCollection<PlannerPresetField> Fields { get; } = new();

    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(); } }
    }

    public string Category
    {
        get => _category;
        set { if (_category != value) { _category = value; OnPropertyChanged(); } }
    }

    public string Summary
    {
        get => _summary;
        set { if (_summary != value) { _summary = value; OnPropertyChanged(); } }
    }

    public string Description
    {
        get => _description;
        set { if (_description != value) { _description = value; OnPropertyChanged(); } }
    }

    public string AccentHex
    {
        get => _accentHex;
        set { if (_accentHex != value) { _accentHex = value; OnPropertyChanged(); } }
    }

    public string IsBlankText
    {
        get => _isBlankText;
        set { if (_isBlankText != value) { _isBlankText = value; OnPropertyChanged(); } }
    }

    public string FieldCountText
    {
        get => _fieldCountText;
        set { if (_fieldCountText != value) { _fieldCountText = value; OnPropertyChanged(); } }
    }

    public string FieldsEmptyText
    {
        get => _fieldsEmptyText;
        set { if (_fieldsEmptyText != value) { _fieldsEmptyText = value; OnPropertyChanged(); } }
    }

    public bool IsBlank
    {
        get => _isBlank;
        set
        {
            if (_isBlank != value)
            {
                _isBlank = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBlankText));
            }
        }
    }

    public bool HasFields
    {
        get => _hasFields;
        set
        {
            if (_hasFields != value)
            {
                _hasFields = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoFields));
            }
        }
    }

    public bool HasNoFields => !HasFields;

    private void RefreshFieldState()
    {
        HasFields = Fields.Count > 0;
        FieldCountText = IsBlank
            ? "Manual setup"
            : $"{Fields.Count} item{(Fields.Count == 1 ? string.Empty : "s")}";
        FieldsEmptyText = "No items are included in this preset.";
    }

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

    public Command StartCommand { get; }
    public Command BackCommand { get; }

    public PlannerTemplateDetailPage(
        IPlannerCatalogService catalogService,
        IAppNavigator appNavigator,
        NavigationDataService navDataService)
    {
        InitializeComponent();

        _catalogService = catalogService;
        _appNavigator = appNavigator;
        _navDataService = navDataService;

        StartCommand = new Command(async () => await StartAsync());
        BackCommand = new Command(async () => await GoBackAsync());

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await LoadPresetAsync();
            _isInitialized = true;
        }
    }

    private async Task LoadPresetAsync()
    {
        try
        {
            IsLoading = true;

            var selectedPreset = _navDataService.Get<PlannerPreset>(Constants.Constants.Planner.Item_ToOpen);
            if (selectedPreset == null)
            {
                await GoBackAsync();
                return;
            }

            _selectedPreset = selectedPreset;

            Name = _selectedPreset.Name;
            Category = _selectedPreset.Category;
            Summary = _selectedPreset.Summary;
            Description = _selectedPreset.Description;
            AccentHex = _selectedPreset.AccentHex;
            IsBlank = _selectedPreset.IsBlank;            

            Fields.Clear();
            foreach (var field in _selectedPreset.Fields.OrderBy(f => f.FieldOrder))
            {
                Fields.Add(field);
            }

            RefreshFieldState();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartAsync()
    {
        if (_selectedPreset == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            await StartSelectedPresetAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Unable to start plan",
                ex.Message,
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartSelectedPresetAsync()
    {
        if (_selectedPreset.IsBlank)
        {
            Preferences.Set(Constants.Constants.AppState.PlannerBypassGateKey, true);
            _appNavigator.NavigateToMainShell();
            return;
        }

        var userId = (byte)Preferences.Get(Constants.Constants.AppUser.CurrentUserId, 0);
        if (userId == 0)
        {
            await DisplayAlert(
                "No active profile",
                "Please select a profile before starting a planner template.",
                "OK");
            return;
        }

        await _catalogService.StartPresetAsync(userId, _selectedPreset);
        _appNavigator.NavigateToMainShell();
    }

    private static async Task GoBackAsync()
    {
        if (Shell.Current?.Navigation != null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        if (Application.Current?.MainPage is NavigationPage navigationPage)
        {
            await navigationPage.Navigation.PopAsync();
            return;
        }

        if (Application.Current?.MainPage?.Navigation != null)
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
