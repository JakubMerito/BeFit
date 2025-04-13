namespace BeFit.Models
{
    public class ExerciseStatistics
    {
        public ExerciseType ExerciseType { get; set; }
        public int Count { get; set; }
        public int TotalRepetitions { get; set; }
        public double AverageWeight { get; set; }
        public double MaxWeight { get; set; }

    }
}
