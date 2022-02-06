using WordleSolverAPI.Logic.Enums;

namespace WordleSolverAPI.Logic.Models
{
    public class LetterPosition
    {
        public string Letter { get; set; }
        public int Position { get; set; }
        public LetterStatus Status { get; set; }

    }
}
