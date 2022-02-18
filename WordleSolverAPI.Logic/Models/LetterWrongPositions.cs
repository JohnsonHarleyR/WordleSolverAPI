using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class LetterWrongPositions
    {
        public string Letter { get; set; }
        public List<int> WrongPositions { get; set; }
        public List<int> PossiblePositions { get; set; }

        public LetterWrongPositions()
        {
            WrongPositions = new List<int>();
            PossiblePositions = new List<int>();
        }
        public LetterWrongPositions(string letter, int firstWrongPosition)
        {
            Letter = letter;
            WrongPositions = new List<int>();
            WrongPositions.Add(firstWrongPosition);
            PossiblePositions = new List<int>();

        }
    }


}
