using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;
using algoBhaiya.ReportBooks.Core.Interfaces;
using System.Text.Json;

namespace algoBhaiya.ReportBook.Presentation.Services
{
    public class PlannerCatalogService : IPlannerCatalogService
    {
        private readonly IRepository<FieldTemplate> _fieldTemplateRepository;
        private readonly IRepository<FieldUnit> _unitRepository;
        private readonly IMonthlyTargetRepository _monthlyTargetRepository;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PlannerCatalogService(
            IRepository<FieldTemplate> fieldTemplateRepository,
            IRepository<FieldUnit> unitRepository,
            IMonthlyTargetRepository monthlyTargetRepository)
        {
            _fieldTemplateRepository = fieldTemplateRepository;
            _unitRepository = unitRepository;
            _monthlyTargetRepository = monthlyTargetRepository;
        }

        public async Task<IReadOnlyList<PlannerPreset>> GetCatalogAsync()
        {
            var presets = await LoadCatalogAsync();

            return presets
                .OrderBy(p => p.SortOrder)
                .ToList();
        }
        
        public async Task<bool> HasActiveFieldsAsync(byte userId)
        {
            var field = await _fieldTemplateRepository.GetFirstOrDefaultAsync(f =>
                f.UserId == userId &&
                !f.IsDeleted);

            return field != null;
        }

        public async Task StartPresetAsync(byte userId, PlannerPreset preset)
        {
            if (preset == null)
            {
                throw new InvalidOperationException("Planner preset not found.");
            }

            if (preset.IsBlank)
            {
                return;
            }

            var today = DateTime.Today;
            var currentMonth = (byte)today.Month;

            var activeFields = (await _fieldTemplateRepository.GetListAsync(f =>
                    f.UserId == userId &&
                    !f.IsDeleted))
                .ToList();

            var existingMonthlyTargets = (await _monthlyTargetRepository.GetMonthlyTargetsAsync(userId, today.Year, currentMonth))
                .ToDictionary(t => t.FieldTemplateId);

            var activeLookup = activeFields.ToDictionary(
                field => BuildFieldKey(field.FieldName, field.UnitId, field.FieldOrder),
                field => field,
                StringComparer.OrdinalIgnoreCase);

            var seededFieldCount = 0;
            var seededTargetCount = 0;

            foreach (var presetField in preset.Fields.OrderBy(f => f.FieldOrder))
            {
                var key = BuildFieldKey(presetField.FieldName, presetField.UnitId, presetField.FieldOrder);
                if (!activeLookup.TryGetValue(key, out var activeField))
                {
                    activeField = new FieldTemplate
                    {
                        UserId = userId,
                        FieldName = presetField.FieldName,
                        UnitId = presetField.UnitId,
                        ValueType = presetField.ValueType,
                        FieldOrder = presetField.FieldOrder,
                        IsEnabled = true,
                        IsDeleted = false
                    };

                    await _fieldTemplateRepository.AddAsync(activeField);
                    seededFieldCount++;
                    activeLookup[key] = activeField;
                }

                var target = existingMonthlyTargets.TryGetValue(activeField.Id, out var existingTarget)
                    ? existingTarget
                    : new MonthlyTarget
                    {
                        UserId = userId,
                        FieldTemplateId = activeField.Id,
                        Month = currentMonth,
                        Year = today.Year,
                        TargetValue = string.Empty,
                        FieldOrder = presetField.FieldOrder,
                        IsDeleted = false
                    };

                target.FieldOrder = presetField.FieldOrder;
                target.IsDeleted = false;
                target.TargetValue = target.TargetValue ?? string.Empty;

                await _monthlyTargetRepository.SaveMonthlyTargetAsync(target);
                seededTargetCount++;
            }
        }

        private async Task<List<PlannerPreset>> LoadCatalogAsync()
        {
            var definitions = await LoadCatalogDefinitionsAsync();
            var unitLookup = await EnsureRequiredUnitsAsync(definitions);

            var presets = definitions
                .OrderBy(p => p.SortOrder)
                .Select(definition =>
                {
                    var fields = definition.Fields
                        .OrderBy(field => field.FieldOrder)
                        .Select(field =>
                        {
                            if (!unitLookup.TryGetValue(field.UnitName, out var unit))
                            {
                                throw new InvalidOperationException($"Planner unit '{field.UnitName}' was not found.");
                            }

                            return new PlannerPresetField
                            {
                                PlannerPresetId = definition.Id,
                                FieldName = field.FieldName,
                                UnitId = unit.Id,
                                ValueType = field.ValueType,
                                FieldOrder = field.FieldOrder,
                                Unit = unit
                            };
                        })
                        .ToList();

                    return new PlannerPreset
                    {
                        Id = definition.Id,
                        Name = definition.Name,
                        Category = definition.Category,
                        Summary = definition.Summary,
                        Description = definition.Description,
                        AccentHex = definition.AccentHex,
                        SortOrder = definition.SortOrder,
                        IsBlank = definition.IsBlank,
                        Fields = fields
                    };
                })
                .ToList();

            return presets;
        }

        private static async Task<List<PlannerPresetCatalogDefinition>> LoadCatalogDefinitionsAsync()
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("planner-presets.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var catalog = JsonSerializer.Deserialize<List<PlannerPresetCatalogDefinition>>(json, _jsonOptions);
            if (catalog == null || catalog.Count == 0)
            {
                throw new InvalidOperationException("Planner preset catalog is empty.");
            }

            return catalog;
        }

        private async Task<Dictionary<string, FieldUnit>> EnsureRequiredUnitsAsync(IEnumerable<PlannerPresetCatalogDefinition> definitions)
        {
            var requiredUnits = definitions
                .SelectMany(definition => definition.Fields)
                .Select(field => new { field.UnitName, field.ValueType })
                .DistinctBy(field => field.UnitName)
                .ToList();

            await FieldUnitSeedGate.Gate.WaitAsync();
            try
            {
                var existingUnits = (await _unitRepository.GetAllAsync()).ToList();
                var unitLookup = existingUnits
                    .Where(unit => !string.IsNullOrWhiteSpace(unit.UnitName))
                    .GroupBy(unit => unit.UnitName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.OrderBy(unit => unit.IsDeleted).ThenBy(unit => unit.Id).First(),
                        StringComparer.OrdinalIgnoreCase);

                foreach (var requiredUnit in requiredUnits)
                {
                    if (string.IsNullOrWhiteSpace(requiredUnit.UnitName))
                    {
                        throw new InvalidOperationException("Planner preset catalog contains a field without a unit name.");
                    }

                    var existingUnit = existingUnits.FirstOrDefault(unit =>
                        string.Equals(unit.UnitName, requiredUnit.UnitName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(unit.ValueType, requiredUnit.ValueType, StringComparison.OrdinalIgnoreCase));

                    if (existingUnit != null)
                    {
                        if (existingUnit.IsDeleted)
                        {
                            existingUnit.IsDeleted = false;
                            await _unitRepository.UpdateAsync(existingUnit);
                        }

                        unitLookup[requiredUnit.UnitName] = existingUnit;
                        continue;
                    }

                    var unit = new FieldUnit
                    {
                        UnitName = requiredUnit.UnitName,
                        ValueType = requiredUnit.ValueType
                    };

                    await _unitRepository.AddAsync(unit);
                    unitLookup[requiredUnit.UnitName] = unit;
                    existingUnits.Add(unit);
                }

                return unitLookup;
            }
            finally
            {
                FieldUnitSeedGate.Gate.Release();
            }
        }

        private static string BuildFieldKey(string fieldName, byte unitId, byte fieldOrder)
        {
            return $"{fieldName?.Trim().ToLowerInvariant()}|{unitId}|{fieldOrder}";
        }

        private sealed class PlannerPresetCatalogDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Summary { get; set; }
            public string Description { get; set; }
            public string AccentHex { get; set; }
            public byte SortOrder { get; set; }
            public bool IsBlank { get; set; }
            public List<PlannerPresetFieldDefinition> Fields { get; set; } = new();
        }

        private sealed class PlannerPresetFieldDefinition
        {
            public string FieldName { get; set; }
            public string UnitName { get; set; }
            public string ValueType { get; set; }
            public byte FieldOrder { get; set; }
                }
            }
        }
