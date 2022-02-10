using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordleSolverAPI.Logic.Models;

namespace WordleSolverAPI.Logic
{
    public static class FailAnalyzer
    {
        public static List<string> GetFailingWords(int startIndex, int howMany)
        {
            List<string> allWords = WordSorter.GetAllWords();
            int endIndex = startIndex + howMany;
            if (endIndex >= allWords.Count)
            {
                endIndex = allWords.Count - 1;
            }

            List<string> failedWord = new List<string>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                string word = allWords[i];
                WordleGuesses result = WordSorter.GuessWordleSolution(word);
                if (!result.DidWin)
                {
                    failedWord.Add(result.CorrectAnswer);
                }
            }
            return failedWord;
        }

        public static List<PositionLetterFrequencies> AnalyzeFailedPositionLetters()
        {
            List<string> failedWords = GetFailedWords();
            List<int> positions = new List<int>() { 0, 1, 2, 3, 4 };
            string[] letters = WordSorter.allLetters;
            List<PositionLetterFrequencies> positionFrequencies = new List<PositionLetterFrequencies>();

            foreach (var position in positions)
            {
                List<LetterCount> letterCounts = new List<LetterCount>();
                for (int i = 0; i < letters.Length; i++)
                {
                    string letter = letters[i];
                    int count = 0;
                    foreach (var word in failedWords)
                    {
                        if (word.Substring(position, 1) == letter)
                        {
                            count++;
                        }
                    }
                    letterCounts.Add(new LetterCount()
                    {
                        Letter = letter,
                        Count = count
                    });
                }
                positionFrequencies.Add(new PositionLetterFrequencies()
                {
                    Position = position,
                    LetterCounts = letterCounts.OrderBy(l => l.Count).Reverse().ToList()
                });
            }

            return positionFrequencies;
        }
        public static List<string> MatchWordsToPattern(List<string> words, string pattern, bool isInclusive)
        {
            List<string> matching = new List<string>();
            List<string> nonMatching = new List<string>();
            string[] patternArray = GetPattern(pattern);

            foreach (var word in words)
            {
                bool doesMatch = true;
                for (int i = 0; i < patternArray.Length; i++)
                {
                    if (patternArray[i] != null &&
                        word.Substring(i, 1) != patternArray[i])
                    {
                        doesMatch = false;
                        break;
                    }
                }

                if (doesMatch)
                {
                    matching.Add(word);
                }
                else
                {
                    nonMatching.Add(word);
                }
            }

            if (isInclusive)
            {
                return matching;
            }
            return nonMatching;
        }

        public static string[] GetPattern(string word)
        {
            string[] pattern = new string[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                string character = word.Substring(i, 0).ToLower();
                if (character == "_")
                {
                    pattern[i] = null;
                }
                else
                {
                    pattern[i] = character;
                }
            }
            return pattern;
        }
        public static List<string> GetFailedWords()
        {
            //string dictionaryPath = $".\\files\\word-list.txt";
            string dictionaryPath = $".\\files\\failed-words.txt";
            //string dictionaryPath = $".\\files\\failed-words2.txt";

            string word;
            List<string> wordList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(dictionaryPath);

                word = sr.ReadLine();

                while (word != null)
                {
                    wordList.Add(word);
                    word = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                wordList.Add($"Error: {e}");
                Console.WriteLine($"Error: {e}");
            }
            return wordList;
        }
    }
}
