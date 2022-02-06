using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class PositionLetterFrequencies
    {
        public int Position { get; set; }
        public List<LetterCount> LetterCounts { get; set; }
    }
}
