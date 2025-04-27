using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldTemplatePage : ContentPage
{
    private readonly IRepository<FieldTemplate> _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationDataService _navDataService;

    private ObservableCollection<FieldTemplate> _templates = new();
    private List<FieldUnit> _availableUnits = new();

    public ObservableCollection<FieldTemplate> Templates => _templates;
    private byte _loggedInUser = 0;
    private bool _isInitialized = false;

    public Command DeleteCommand { get; }

    public FieldTemplatePage(
        IServiceProvider serviceProvider,
        IRepository<FieldTemplate> repository,
        NavigationDataService navDataService)
    {
        InitializeComponent();
        BindingContext = this;
        _repository = repository;
        _serviceProvider = serviceProvider;
        _navDataService = navDataService;

        _loggedInUser = (byte)Preferences.Get("CurrentUserId", 0);

        DeleteCommand = new Command<FieldTemplate>(async (field) =>
        {
            var confirm = await DisplayAlert("Delete", $"Delete '{field.FieldName}'?", "Yes", "No");
            if (confirm)
            {
                bool isDeletionLocked = false;

                // Check in monthly plan
                var monthlyPlan = await _serviceProvider
                    .GetRequiredService<IRepository<MonthlyTarget>>()
                    .GetListAsync(t => 
                        t.FieldTemplateId == field.Id &&
                        t.UserId == _loggedInUser);

                isDeletionLocked = monthlyPlan.Count() > 0;

                // Check in daily report
                if (!isDeletionLocked)
                {
                    var dailyReports = await _serviceProvider
                        .GetRequiredService<IRepository<DailyEntry>>()
                        .GetListAsync(d => 
                            d.FieldTemplateId == field.Id &&
                            d.UserId == _loggedInUser);

                    isDeletionLocked = dailyReports.Count() > 0;
                }
                
                // Ignore if field is in Use.
                if (isDeletionLocked)
                {
                    await Shell.Current.DisplayAlert(
                        "Field In Use",
                        "This Field is used in daily reports and can't be deleted.",
                        "OK");
                    return;
                }

                await _repository.DeleteAsync(field.Id);
                Templates.Remove(field);
            }
        });
       
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            try
            {
                await LoadTemplatesAsync(); // Only after page fully loaded
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                // Log exception
            }
        }
    }

    private async Task LoadTemplatesAsync()
    {
        _availableUnits = (await _serviceProvider.GetRequiredService<IRepository<FieldUnit>>()
                                    .GetListAsync(u => u.IsDeleted == false))
                                    .ToList();
        
        if (_loggedInUser == 0) return;

        var templates = (await _repository
                .GetListAsync(t => 
                    t.UserId == _loggedInUser &&
                    t.IsDeleted == false)
                )
                .OrderByDescending(t => t.IsEnabled)
                .ThenBy(t => t.FieldOrder);

        foreach (var tpl in templates)
            tpl.Unit = _availableUnits.FirstOrDefault(u => u.Id == tpl.UnitId);

        _templates.Clear();
        foreach (var tpl in templates)
            _templates.Add(tpl);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        _navDataService.Set(Constants.Constants.FieldTemplate.Action_OnUnitSaved, (Action<FieldTemplate, FieldTemplate>)OnUnitSaved);

        await OpenModalAsync();
    }

    public Command<FieldTemplate> OpenDetailsCommand => new Command<FieldTemplate>(OnFieldTapped);

    private async void OnFieldTapped(FieldTemplate tappedTemplate)
    {
        _navDataService.Set(Constants.Constants.FieldTemplate.Item_ToEdit, tappedTemplate);
        _navDataService.Set(Constants.Constants.FieldTemplate.Action_OnUnitSaved, (Action<FieldTemplate, FieldTemplate>)OnUnitSaved);

        await OpenModalAsync();

    }
    
    private async void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (sender is Switch sw && sw.BindingContext is FieldTemplate template)
        {
            if (template.IsEnabled == e.Value)
            {
                return;
            }

            bool requestedState = e.Value;
            string confirmationText = requestedState ? "Enable" : "Disable";

            bool confirm = await DisplayAlert("Confirmation", $"Do you want to '{confirmationText}'?", "Yes", "No");

            if (confirm)
            {
                template.IsEnabled = requestedState;
                await _repository.UpdateAsync(template);                
            }
            else
            {
                // Revert the switch to the original state
                sw.Toggled -= OnSwitchToggled; // prevent recursive trigger
                sw.IsToggled = template.IsEnabled;
                sw.Toggled += OnSwitchToggled;
            }
        }
    }

    private async Task OpenModalAsync()
    {
        var fieldPage = _serviceProvider.GetRequiredService<FieldTemplateDetailPage>();
        
        await Navigation.PushModalAsync(fieldPage);

        await fieldPage.ResultSource.Task;   // wait, until completing the task.

        await Navigation.PopModalAsync();
    }

    private void OnUnitSaved(FieldTemplate oldField, FieldTemplate newField)
    {
        var existing = Templates.FirstOrDefault(x => x.Id == oldField.Id);
        if (existing != null)
        {
            var index = Templates.IndexOf(existing);
            Templates[index] = newField; // updates item in-place           
        }
        else
        {
            Templates.Add(newField);
        }
    }
}
