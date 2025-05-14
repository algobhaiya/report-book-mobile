namespace algoBhaiya.ReportBook.Presentation.Constants
{
    public static class Constants
    {
        public static class FieldUnit
        {
            public const string Item_ToEdit = "FieldUnit_Item_ToEdit";
            public const string Action_OnUnitSaved = "FieldUnit_Action_OnUnitSaved";
        }

        public static class UnitType
        {
            public const string Double = "double";
            public const string Bool = "bool";
        }

        public static class FieldTemplate
        {
            public const string Item_ToEdit = "FieldTemplate_Item_ToEdit";
            public const string Action_OnUnitSaved = "FieldTemplate_Action_OnUnitSaved";
        }

        public static class DailyEntry
        {
            public const string Item_SelectedDate = "DailyEntry_Item_SelectedDate";
        }

        public static class AppUser
        {
            public const string CurrentUserId = "CurrentUserId";
        }

        public static class Setting
        {
            public const string ModificationDuration = "Setting_ModificationDuration";
            public const string DataRemovalPeriod = "Setting_DataRemovalPeriod";
        }

        public static class AppState
        {
            public const string LastCleanupDateKey = "last_cleanup_date";
            public const string FirstRunKey = "IsSeedingInitialDataCompleted";
        }

        public static class LogIn
        {
            public const string SoftDeleteBtn = "Hide/Remove User (Soft Delete)";
            public const string HardDeleteBtn = "Delete Permanently (Everything from device)";
        }
    }
}
