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
                var templates = await _serviceProvider.GetRequiredService<IRepository<FieldTemplate>>().GetListAsync(t => t.UnitId == unit.Id);                

                if (templates.Count() > 0)
                {
                    await Shell.Current.DisplayAlert("Error", "This unit already in use", "OK");
                    return;
                }
                await _repository.DeleteAsync(unit.Id);
                Units.Remove(unit);
            }
        });

        // Load saved units
        LoadUnits();        
    }

    private async void LoadUnits()
    {
        var units = await _repository.GetListAsync(u => u.IsDeleted == false);
        _units.Clear();
        foreach (var unit in units)
            _units.Add(unit);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        
        _navDataService.Set(Constants.Constants.FieldUnit.Action_OnUnitSaved, (Action<FieldUnit, FieldUnit>)OnUnitSaved);
        
        var unitPage = _serviceProvider.GetRequiredService<FieldUnitAddEditPage>();
        await Shell.Current.Navigation.PushAsync(unitPage);
    }

    private async void OnUnitTapped(FieldUnit tappedUnit)
    {
        _navDataService.Set(Constants.Constants.FieldUnit.Item_ToEdit, tappedUnit);
        _navDataService.Set(Constants.Constants.FieldUnit.Action_OnUnitSaved, (Action<FieldUnit, FieldUnit>)OnUnitSaved);
        
        var unitPage = _serviceProvider.GetRequiredService<FieldUnitAddEditPage>();
        await Shell.Current.Navigation.PushAsync(unitPage);
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
