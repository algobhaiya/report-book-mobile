using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Presentation.ViewModels;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Collections.ObjectModel;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class FieldUnitPage : ContentPage
{
    private readonly IRepository<FieldUnit> _repository;
    private readonly IServiceProvider _serviceProvider;
    private ObservableCollection<FieldUnit> _units = new();

    public ObservableCollection<FieldUnit> Units => _units;

    public FieldUnitPage(
        IRepository<FieldUnit> repository, 
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = this;

        _repository = repository;

        // Load saved units
        LoadUnits();
        _serviceProvider = serviceProvider;
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
        var unitViewModel = _serviceProvider.GetRequiredService<FieldUnitAddEditViewModel>();
        unitViewModel.AssignEntryAsync(null);

        var unitPage = _serviceProvider.GetRequiredService<FieldUnitAddEditPage>();
        await Shell.Current.Navigation.PushAsync(unitPage);
    }

    public Command<FieldUnit> OpenDetailsCommand => new Command<FieldUnit>(OnUnitTapped);

    private async void OnUnitTapped(FieldUnit tappedUnit)
    {
        var unitViewModel = _serviceProvider.GetRequiredService<FieldUnitAddEditViewModel>();        
        unitViewModel.AssignEntryAsync(tappedUnit);

        var unitPage = _serviceProvider.GetRequiredService<FieldUnitAddEditPage>();
        await Shell.Current.Navigation.PushAsync(unitPage);       
    }
}
