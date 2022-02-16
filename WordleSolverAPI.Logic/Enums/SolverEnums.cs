namespace WordleSolverAPI.Logic.Enums
{
    public enum LetterStatus
    {
        Correct,
        WrongPosition,
        Incorrect
    }

    public enum StartMode
    {
        Guess,
        Strict
    }

    public enum SolveMode
    {
        Normal,
        Turbo,
        Pattern,
        Letters
    }

    public enum WordListType
    {
        Contest,
        Full,
        Wordle
    }
}
