using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    public class SeedDataService : ISeedDataService
    {
        private readonly string FirstRunKey;
        private readonly IServiceProvider _serviceProvider;

        public SeedDataService(
            IServiceProvider serviceProvider,
            IDailyEntryRepository dailyEntryRepository)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException();
            FirstRunKey = Constants.Constants.AppState.FirstRunKey;
        }
        
        public async Task SeedDefaultUnitsAsync()
        {
            bool isFirstRun = !Preferences.Get(FirstRunKey, false);

            if (isFirstRun)
            {
                var unitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

                if (unitRepo != null)
                {
                    var defaultUnits = new List<FieldUnit>
                    {
                        new FieldUnit { UnitName = "Hours", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Minutes", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Days", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Checkbox", ValueType = Constants.Constants.UnitType.Bool },
                        new FieldUnit { UnitName = "Pages", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Ayat", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Count", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Persons", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Kg", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Km", ValueType = Constants.Constants.UnitType.Double },
                        new FieldUnit { UnitName = "Times", ValueType = Constants.Constants.UnitType.Double }
                    };

                    if (defaultUnits.Count > 0 )
                    {
                        await unitRepo.InsertAllAsync(defaultUnits);
                    }
                }

                Preferences.Set(FirstRunKey, true);
            }
        }
    }

}
