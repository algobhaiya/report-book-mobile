using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using AppConstants = algoBhaiya.ReportBook.Presentation.Constants.Constants;

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
            FirstRunKey = AppConstants.AppState.FirstRunKey;
        }

        public async Task SeedDefaultUnitsAsync()
        {
            bool isFirstRun = !Preferences.Get(FirstRunKey, false);

            if (!isFirstRun)
            {
                return;
            }

            await FieldUnitSeedGate.Gate.WaitAsync();
            try
            {
                var unitRepo = _serviceProvider.GetRequiredService<IRepository<FieldUnit>>();

                var defaultUnits = new List<FieldUnit>
                {
                    new FieldUnit { UnitName = "Hours", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Minutes", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Days", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Checkbox", ValueType = AppConstants.UnitType.Bool },
                    new FieldUnit { UnitName = "Pages", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Ayat", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Count", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Persons", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Kg", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Km", ValueType = AppConstants.UnitType.Double },
                    new FieldUnit { UnitName = "Times", ValueType = AppConstants.UnitType.Double }
                };

                var existingNames = new HashSet<string>(
                    (await unitRepo.GetAllAsync()).Select(u => u.UnitName ?? string.Empty),
                    StringComparer.OrdinalIgnoreCase);

                var missingUnits = defaultUnits
                    .Where(unit => !existingNames.Contains(unit.UnitName ?? string.Empty))
                    .ToList();

                if (missingUnits.Count > 0)
                {
                    await unitRepo.InsertAllAsync(missingUnits);
                }

                Preferences.Set(FirstRunKey, true);
            }
            finally
            {
                FieldUnitSeedGate.Gate.Release();
            }
        }
    }
}
