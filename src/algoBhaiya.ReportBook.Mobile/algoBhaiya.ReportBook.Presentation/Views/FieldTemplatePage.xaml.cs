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

        LoadTemplates();        
    }

    private async void LoadTemplates()
    {
        _availableUnits = (await _serviceProvider.GetRequiredService<IRepository<FieldUnit>>()
                                    .GetListAsync(u => u.IsDeleted == false))
                                    .ToList();
        
        if (_loggedInUser == 0) return;

        var templates = (await _repository
                .GetListAsync(t => t.UserId == _loggedInUser)
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

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is FieldTemplate template)
        {
            string newName = await DisplayPromptAsync("Edit Field", "Update field name:", initialValue: template.FieldName);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                template.FieldName = newName;
                await _repository.UpdateAsync(template);
                LoadTemplates(); // Refresh
            }
        }
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
