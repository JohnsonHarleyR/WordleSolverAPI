namespace WordleSolverAPI.Logic.Models
{
    public class WordGuess
    {
        public int GuessNumber { get; set; }
        public string Word { get; set; }
        public LetterPosition[] Result { get; }
        public bool IsCorrect { get; set; }

        public WordGuess(string word)
        {
            Word = word;
            Result = new LetterPosition[5];
        }
    }
}
