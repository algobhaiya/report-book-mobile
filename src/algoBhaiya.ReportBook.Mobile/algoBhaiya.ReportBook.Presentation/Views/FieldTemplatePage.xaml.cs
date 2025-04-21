using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldTemplatePage : ContentPage
{
    private readonly IRepository<FieldTemplate> _repository;
    private readonly IServiceProvider _serviceProvider;

    private ObservableCollection<FieldTemplate> _templates = new();
    private List<FieldUnit> _availableUnits = new();

    public ObservableCollection<FieldTemplate> Templates => _templates;
    private int _loggedInUser = -1;

    public FieldTemplatePage(
        IServiceProvider serviceProvider,
        IRepository<FieldTemplate> repository)
    {
        InitializeComponent();
        BindingContext = this;
        _repository = repository;
        _serviceProvider = serviceProvider;

        _loggedInUser = Preferences.Get("CurrentUserId", -1);

        LoadTemplates();
    }

    private async void LoadTemplates()
    {
        _availableUnits = (await _serviceProvider.GetRequiredService<IRepository<FieldUnit>>()
                                    .GetListAsync(u => u.IsDeleted == false))
                                    .ToList();
        
        if (_loggedInUser == -1) return;

        var templates = await _repository.GetListAsync(t => t.UserId == _loggedInUser);
        
        foreach (var tpl in templates)
            tpl.Unit = _availableUnits.FirstOrDefault(u => u.Id == tpl.UnitId);

        _templates.Clear();
        foreach (var tpl in templates)
            _templates.Add(tpl);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("New Field", "Enter field name:");
        
        var unitNames = _availableUnits.Select(u => u.UnitName).ToArray();
        string unitName = await DisplayActionSheet("Select Unit", "Cancel", null, unitNames);

        if (!string.IsNullOrWhiteSpace(name) && unitName != "Cancel")
        {
            var selectedUnit = _availableUnits.FirstOrDefault(u => u.UnitName == unitName);
            var tpl = new FieldTemplate 
            { 
                FieldName = name, 
                UnitId = selectedUnit.Id, 
                Unit = selectedUnit,
                UserId = _loggedInUser
            };
            await _repository.AddAsync(tpl);
            _templates.Add(tpl);
        }
    }

    public Command<FieldTemplate> OpenDetailsCommand => new Command<FieldTemplate>(OnFieldTapped);

    private async void OnFieldTapped(FieldTemplate tappedTemplate)
    {
        await DisplayAlert("Field Details",
            $"Name: {tappedTemplate.FieldName}\n" +
            $"Unit: {tappedTemplate.Unit?.UnitName ?? "None"}\n" +
            $"Type: {tappedTemplate.ValueType ?? "N/A"}", "OK");
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

    //private async void OnSwitchToggled(object sender, ToggledEventArgs e)
    //{
    //    bool isOn = e.Value;

    //    var confirmationText = isOn ? "Enable" : "Disable";
    //    var confirm = await DisplayAlert("Confirmation", $"Do you want to '{confirmationText}'?", "Yes", "No");
    //    if (confirm)
    //    {
    //        // Handle the toggle state
    //        if (isOn)
    //        {
    //            if (sender is Button btn && btn.BindingContext is FieldTemplate template)
    //            {
    //                template.IsEnabled = true;
    //                await _repository.UpdateAsync(template);
    //                LoadTemplates(); // Refresh
    //            }
    //        }
    //        else
    //        {
    //            if (sender is Button btn && btn.BindingContext is FieldTemplate template)
    //            {
    //                template.IsEnabled = false;
    //                await _repository.UpdateAsync(template);
    //                LoadTemplates(); // Refresh
    //            }
    //        }
    //    }
    //}

}
