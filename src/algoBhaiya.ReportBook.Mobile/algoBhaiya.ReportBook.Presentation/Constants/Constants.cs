namespace algoBhaiya.ReportBook.Presentation.Constants
{
    public static class Constants
    {
        public static class FieldUnit
        {
            public static string Item_ToEdit = "FieldUnit_Item_ToEdit";
            public static string Action_OnUnitSaved = "FieldUnit_Action_OnUnitSaved";
        }

        public static class UnitType
        {
            public static string Double = "double";
            public static string Bool = "bool";
        }

        public static class FieldTemplate
        {
            public static string Item_ToEdit = "FieldTemplate_Item_ToEdit";
            public static string Action_OnUnitSaved = "FieldTemplate_Action_OnUnitSaved";
        }

        public static class DailyEntry
        {
            public static string Item_SelectedDate = "DailyEntry_Item_SelectedDate";
        }

        public static class AppUser
        {
            public static string CurrentUserId = "CurrentUserId";
        }

        public static class Setting
        {
            public static string ModificationDuration = "Setting_ModificationDuration";
            public static string DataRemovalPeriod = "Setting_DataRemovalPeriod";
        }

        public static class AppState
        {
            public static string LastCleanupDateKey = "last_cleanup_date";
            public static string FirstRunKey = "IsSeedingInitialDataCompleted";
        }

        public static class LogIn
        {
            public const string SoftDeleteBtn = "Hide/Remove User (Soft Delete)";
            public const string HardDeleteBtn = "Delete Permanently (Everything from device)";
        }
    }
}
