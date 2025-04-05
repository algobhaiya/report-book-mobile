
using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Helpers
{
    public class FieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate DoubleTemplate { get; set; }
        public DataTemplate BoolTemplate { get; set; }

        public FieldTemplateSelector()
        {
            IntTemplate = new DataTemplate(() => CreateEntry("Number"));
            DoubleTemplate = new DataTemplate(() => CreateEntry("Decimal"));
            BoolTemplate = new DataTemplate(() => CreateCheckbox());
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var field = item as DailyEntryFieldViewModel;
            return field.ValueType switch
            {
                "int" => IntTemplate,
                "double" => DoubleTemplate,
                "bool" => BoolTemplate,
                _ => IntTemplate
            };
        }

        private ViewCell CreateEntry(string keyboard)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, "FieldName");

            var entry = new Entry { Keyboard = keyboard == "Number" ? Keyboard.Numeric : Keyboard.Default };
            entry.SetBinding(Entry.TextProperty, "Value");

            var unitLabel = new Label { Margin = new Thickness(5, 0, 0, 0) };
            unitLabel.SetBinding(Label.TextProperty, "UnitName");

            return new ViewCell
            {
                View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { label, entry, unitLabel }
                }
            };
        }

        private ViewCell CreateCheckbox()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, "FieldName");

            var checkbox = new CheckBox();
            checkbox.SetBinding(CheckBox.IsCheckedProperty, "Value", BindingMode.TwoWay, converter: new BoolStringConverter());

            return new ViewCell
            {
                View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { label, checkbox }
                }
            };
        }
    }

}
