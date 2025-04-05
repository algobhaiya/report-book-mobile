using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitPage : ContentPage
{
    private readonly IRepository<FieldUnit> _repository;
    private ObservableCollection<FieldUnit> _units = new();

    public ObservableCollection<FieldUnit> Units => _units;

    public FieldUnitPage(
        IRepository<FieldUnit> repository)
    {
        InitializeComponent();
        BindingContext = this;

        _repository = repository;

        // Load saved units
        LoadUnits();
    }

    private async void LoadUnits()
    {
        var units = await _repository.GetAllAsync();
        _units.Clear();
        foreach (var unit in units)
            _units.Add(unit);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("New Unit", "Enter unit name:");
        string[] types = new[] { "int", "double", "bool" };
        string type = await DisplayActionSheet("Select Value Type", "Cancel", null, types);

        if (!string.IsNullOrWhiteSpace(name) && type != "Cancel")
        {
            var unit = new FieldUnit { UnitName = name, ValueType = type };
            await _repository.AddAsync(unit);
            _units.Add(unit);
        }
    }
}
