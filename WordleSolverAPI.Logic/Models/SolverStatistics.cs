using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class SolverStatistics
    {
        public double CorrectAnswerRate { get; set; }
        public double AverageGuessesToWin { get; set; }
        public double AverageMilliseconds { get; set; }
        public List<string> FailedWords { get; set; }
        public List<string> ErrorWords { get; set; }
        public double CorrectAnswerRateNoErrors { get; set; }
    }
}

