

using algoBhaiya.ReportBook.Core.Entities;
using algoBhaiya.ReportBook.Core.Interfaces;

namespace algoBhaiya.ReportBook.Presentation.Views
{
    
    public partial class DailyEntryPage : ContentPage
    {
        private readonly IDailyProductivityRepository _repository;

        public DailyEntryPage(
            IDailyProductivityRepository repository)
        {
            InitializeComponent();
            _repository = repository;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (double.TryParse(StudyHoursEntry.Text, out double studyHours) &&
                int.TryParse(ExerciseMinutesEntry.Text, out int exerciseMinutes) &&
                int.TryParse(TeaCountEntry.Text, out int teaCount))
            {
                var entry = new DailyProductivityEntry
                {
                    Date = DateTime.Today,
                    StudyHours = studyHours,
                    ExerciseMinutes = exerciseMinutes,
                    TeaCount = teaCount
                };

                await _repository.SaveDailyEntryAsync(entry);
                await DisplayAlert("Success", "Entry saved successfully.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Please enter valid numbers in all fields.", "OK");
            }
        }
    }
}