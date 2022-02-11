using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class SolverStatistics
    {
        public double CorrectAnswerRate { get; set; }
        public double AverageGuessesToWin { get; set; }
        public double AverageMilliseconds { get; set; }
        public double AverageRemainingAt3Success { get; set; }
        public double AverageRemainingAt3Fail { get; set; }
        public List<string> FailedWords { get; set; }
        public List<string> ErrorWords { get; set; }
        public List<string> FailedWordsNotOnFailList { get; set; }
        public List<string> NewSuccessWords { get; set; }
        public double CorrectAnswerRateNoErrors { get; set; }
    }
}

