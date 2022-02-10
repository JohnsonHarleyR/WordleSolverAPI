using System;
using System.Collections.Generic;

namespace WordleSolverAPI.Logic.Models
{
    public class WordleGuesses
    {
        public string CorrectAnswer { get; set; }
        public bool DidWin { get; set; }
        public int GuessCount { get; set; }
        public TimeSpan TimeToSolve { get; set; }
        public List<WordGuess> Guesses { get; set; }
        public int RemainingPossibleAt3 { get; set; }
        public bool IsFinished { get; set; }
        public bool HasError { get; set; }

        public WordleGuesses()
        {
            DidWin = false;
            Guesses = new List<WordGuess>();
            GuessCount = 0;
            HasError = true;
        }


    }
}
