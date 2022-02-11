using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class PatternAnalysis
    {
        public string StartingPattern { get; set; }
        public double PercentOfWordsWithStartingPattern { get; set; }
        public List<StringPercentContainer> FurtherPatternStatistics { get; set; }
        public List<string> WordsWithStartingPattern { get; set; }
    }
}
