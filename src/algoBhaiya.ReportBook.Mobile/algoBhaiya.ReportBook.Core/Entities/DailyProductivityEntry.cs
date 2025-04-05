
namespace algoBhaiya.ReportBook.Core.Entities
{
    public class DailyProductivityEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double StudyHours { get; set; }
        public int ExerciseMinutes { get; set; }
        public int TeaCount { get; set; }
    }
}
