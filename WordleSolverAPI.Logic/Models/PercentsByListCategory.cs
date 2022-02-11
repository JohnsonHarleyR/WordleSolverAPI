namespace WordleSolverAPI.Logic.Models
{
    public class PercentsByListCategory
    {
        public string Letters { get; set; }
        public bool? DoesContain { get; set; }
        public double PercentOfAllWords { get; set; }
        public double PercentOfSuccessWords { get; set; }
        public double PercentOfFailedWords { get; set; }
    }
}
