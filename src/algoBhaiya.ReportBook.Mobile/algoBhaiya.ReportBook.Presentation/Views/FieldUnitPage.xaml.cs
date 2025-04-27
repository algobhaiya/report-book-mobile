using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Presentation.Helpers;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitPage : ContentPage
{
    private readonly IRepository<FieldUnit> _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationDataService _navDataService;
    
    private ObservableCollection<FieldUnit> _units = new();

    public ObservableCollection<FieldUnit> Units => _units;

    public Command<FieldUnit> OpenDetailsCommand => new Command<FieldUnit>(OnUnitTapped);
    public Command DeleteCommand { get; }

    public FieldUnitPage(
        IRepository<FieldUnit> repository, 
        IServiceProvider serviceProvider,
        NavigationDataService navDataService)
    {
        InitializeComponent();
        BindingContext = this;

        _repository = repository;
        _serviceProvider = serviceProvider;
        _navDataService = navDataService;

        DeleteCommand = new Command<FieldUnit>(async (unit) =>
        {
            var confirm = await DisplayAlert("Delete", $"Delete '{unit.UnitName}'?", "Yes", "No");
            if (confirm)
            {
                bool isDeletionLocked = false;
                bool hasActiveTemplate = false;

                // Check in field templates
                var templates = await _serviceProvider
                    .GetRequiredService<IRepository<FieldTemplate>>()
                    .GetListAsync(t => t.UnitId == unit.Id);                

                if (templates.Count() > 0)
                {
                    isDeletionLocked = true;

                    hasActiveTemplate = templates.Any(t => t.IsDeleted == false);
                }

                // NoDelete: if unit is used in active field.
                if (hasActiveTemplate)
                {
                    await Shell.Current.DisplayAlert(
                        "Unit In Use",
                        "This unit is used in templates and can't be deleted.",
                        "OK");
                    return;
                }

                // SoftDelete: if unit is used in inactive field.
                if (isDeletionLocked)
                {
                    unit.IsDeleted = true;
                    await _repository.UpdateAsync(unit);
                }
                else
                {
                    await _repository.DeleteAsync(unit.Id);
                }
                
                Units.Remove(unit);
            }
        });

        // Load saved units
        LoadUnits();        
    }

    private async void LoadUnits()
    {
        var units = (await _repository
            .GetListAsync(u => u.IsDeleted == false)
            ).OrderBy(u => u.UnitName);

        _units.Clear();

        foreach (var unit in units)
            _units.Add(unit);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        
        _navDataService.Set(Constants.Constants.FieldUnit.Action_OnUnitSaved, (Action<FieldUnit, FieldUnit>)OnUnitSaved);
        
        await OpenModalAsync();
    }

    private async void OnUnitTapped(FieldUnit tappedUnit)
    {
        _navDataService.Set(Constants.Constants.FieldUnit.Item_ToEdit, tappedUnit);
        _navDataService.Set(Constants.Constants.FieldUnit.Action_OnUnitSaved, (Action<FieldUnit, FieldUnit>)OnUnitSaved);

        await OpenModalAsync();
    }

    private async Task OpenModalAsync()
    {
        var unitPage = _serviceProvider.GetRequiredService<FieldUnitAddEditPage>();

        await Navigation.PushModalAsync(unitPage);

        await unitPage.ResultSource.Task;   // wait, until completing the task.

        await Navigation.PopModalAsync();
    }

    private void OnUnitSaved(FieldUnit oldUnit, FieldUnit newUnit)
    {
        var existing = Units.FirstOrDefault(x => x.Id == oldUnit.Id);
        if (existing != null)
        {
            var index = Units.IndexOf(existing);
            Units[index] = newUnit; // updates item in-place           
        }
        else
        {
            Units.Add(newUnit);
        }
    }
}
