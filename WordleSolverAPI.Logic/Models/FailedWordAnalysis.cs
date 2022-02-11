using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class FailedWordAnalysis
    {
        public double PercentOfAllWordsWithDoubles { get; set; }
        public double PercentOfSuccessWordsWithDoubles { get; set; }
        public double PercentOfFailedWordsWithDoubles { get; set; }
        public double PercentOfAllWordsWithMultipleLetter { get; set; }
        public double PercentOfSuccessWordsWithMultipleLetter { get; set; }
        public double PercentOfFailedWordsWithMultipleLetter { get; set; }
        public List<PercentsByListCategory> DoubleLetterPercentsByListType { get; set; }
    }
}
