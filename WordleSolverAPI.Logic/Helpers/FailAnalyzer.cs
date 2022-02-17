using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordleSolverAPI.Logic.Enums;
using WordleSolverAPI.Logic.Models;

namespace WordleSolverAPI.Logic
{
    public static class FailAnalyzer
    {
        /// <summary>
        /// Takes a list of words that are failing by the original algorithm, in order to analyze ways to improve it.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFailedWords(WordListType listType = WordListType.Scrabble)
        {
            //string dictionaryPath = $".\\files\\word-list.txt";
            string dictionaryPath;

            switch (listType)
            {
                default:
                case WordListType.Scrabble:
                    dictionaryPath = $".\\files\\failed-words-scrabble.txt";
                    break;
                case WordListType.Wordle:
                    dictionaryPath = $".\\files\\failed-words-wordle.txt";
                    break;
                case WordListType.Suggested:
                    dictionaryPath = $".\\files\\failed-words.txt";
                    break;
                case WordListType.Full:
                    dictionaryPath = $".\\files\\failed-words.txt";
                    break;
            }

            string word;
            List<string> wordList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(dictionaryPath);

                word = sr.ReadLine();

                while (word != null)
                {
                    word = word.Trim().ToLower();
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

        /// <summary>
        /// Takes a list of possible words and a list of failed words. 
        /// It then removes and words from the list of failed words that is not included in the list of possible words.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPossibleFailedWords(List<string> possibleWords, List<string> failingWords)
        {
            List<string> newFailWordList = failingWords.Where(failingWord =>
            possibleWords.Contains(failingWord)).ToList();

            return newFailWordList;
        }

        public static List<string> GetListWithoutFailedWords(List<string> wordList, List<string> failingWords)
        {
            List<string> newWordList = wordList.Where(word =>
            !failingWords.Contains(word)).ToList();

            return newWordList;
        }

        public static FailedWordAnalysis GetFailedWordAnalysis()
        {
            List<string> allWords = WordSorter.GetAllWords();
            List<string> failedWords = GetFailedWords();
            List<string> wordsWithoutFailed = GetListWithoutFailedWords(allWords, failedWords);

            // Create percent of double letters information
            int allWordsCount = allWords.Count;
            int wordsWithoutFailedCount = wordsWithoutFailed.Count;
            int failedWordsCount = failedWords.Count;
            List<LetterCount> doubleLetterCountsInAllWords = GetDoubleLetterCounts(allWords);
            List<LetterCount> doubleLetterCountsInWordsWithoutFailed = GetDoubleLetterCounts(wordsWithoutFailed);
            List<LetterCount> doubleLetterCountsInFailedWords = GetDoubleLetterCounts(failedWords);
            List<PercentsByListCategory> doubleLetterPercentsByCategory = new List<PercentsByListCategory>();
            foreach (var doubleLetterCountInAllWords in doubleLetterCountsInAllWords)
            {
                LetterCount doubleLetterCountInWordsWithoutFailed = doubleLetterCountsInWordsWithoutFailed.Where(
                    letterCount => letterCount.Letter == doubleLetterCountInAllWords.Letter).FirstOrDefault();

                LetterCount doubleLetterCountInFailedWords = doubleLetterCountsInFailedWords.Where(
                    letterCount => letterCount.Letter == doubleLetterCountInAllWords.Letter).FirstOrDefault();

                PercentsByListCategory newPercents = new PercentsByListCategory()
                {
                    Letters = doubleLetterCountInAllWords.Letter,
                    PercentOfAllWords = doubleLetterCountInAllWords != null ?
                            GetPercent(doubleLetterCountInAllWords.Count, allWordsCount) : 0.0,
                    PercentOfSuccessWords = doubleLetterCountInWordsWithoutFailed != null ?
                            GetPercent(doubleLetterCountInWordsWithoutFailed.Count, wordsWithoutFailedCount) : 0,
                    PercentOfFailedWords = doubleLetterCountInFailedWords != null ?
                            GetPercent(doubleLetterCountInFailedWords.Count, failedWordsCount) : 0
                };
                doubleLetterPercentsByCategory.Add(newPercents);
            }

            FailedWordAnalysis statistics = new FailedWordAnalysis()
            {
                PercentOfAllWordsWithDoubles = GetPercent(GetWordsWithDoubleLetters(allWords).Count,
                allWords.Count),
                PercentOfSuccessWordsWithDoubles = GetPercent(GetWordsWithDoubleLetters(wordsWithoutFailed).Count,
                wordsWithoutFailed.Count),
                PercentOfFailedWordsWithDoubles = GetPercent(GetWordsWithDoubleLetters(failedWords).Count,
                failedWords.Count),
                PercentOfAllWordsWithMultipleLetter = GetPercent(GetWordsWithMultipleLetter(allWords).Count,
                allWords.Count),
                PercentOfSuccessWordsWithMultipleLetter = GetPercent(GetWordsWithMultipleLetter(wordsWithoutFailed).Count,
                wordsWithoutFailed.Count),
                PercentOfFailedWordsWithMultipleLetter = GetPercent(GetWordsWithMultipleLetter(failedWords).Count,
                failedWords.Count),
                DoubleLetterPercentsByListType = doubleLetterPercentsByCategory.OrderBy(p => p.PercentOfFailedWords).Reverse().ToList()
            };

            return statistics;
        }

        public static List<string> GetFailinggWordsThatMatchPatternFromFailedList(string pattern)
        {
            List<string> allFailedWords = GetFailedWords();
            List<string> matchingWords = MatchWordsToPattern(allFailedWords, pattern, true);
            List<string> finalFailList = new List<string>();

            // now attempt to solve matching words to see any that are still failing
            foreach (var word in matchingWords)
            {
                WordleGuesses solution = WordSorter.GuessWordleSolution(word);
                if (solution.DidWin == false)
                {
                    finalFailList.Add(word);
                }
            }
            return finalFailList;
        }

        public static double GetPercentOfWordsWithPatternNowPassing(string pattern)
        {
            List<string> allFailedWords = GetFailedWords();
            List<string> matchingWords = MatchWordsToPattern(allFailedWords, pattern, true);
            List<string> finalPassList = new List<string>();

            // now attempt to solve matching words to see any that are still failing
            foreach (var word in matchingWords)
            {
                WordleGuesses solution = WordSorter.GuessWordleSolution(word);
                if (solution.DidWin)
                {
                    finalPassList.Add(word);
                }
            }
            return GetPercent(finalPassList.Count, matchingWords.Count);
        }

        public static StringPercentContainer GetWordsWithPatternNowPassing(string pattern)
        {
            List<string> allFailedWords = GetFailedWords();
            List<string> matchingWords = MatchWordsToPattern(allFailedWords, pattern, true);
            List<string> finalPassList = new List<string>();

            // now attempt to solve matching words to see any that are still failing
            foreach (var word in matchingWords)
            {
                WordleGuesses solution = WordSorter.GuessWordleSolution(word);
                if (solution.DidWin)
                {
                    finalPassList.Add(word);
                }
            }

            StringPercentContainer result = new StringPercentContainer()
            {
                String = "now passing",
                Percent = (double)finalPassList.Count,
                //Percent = GetPercent(finalPassList.Count, matchingWords.Count),
                ExampleList = finalPassList
            };

            return result;
        }

        public static PatternAnalysis GetPatternsByPercents(List<string> words, bool includeExamples = false)
        {
            List<StringPercentContainer> patternPercents = new List<StringPercentContainer>();

            // determine existing patterns
            List<string> allPatterns = new List<string>();
            for (int i = 0; i < words.Count - 1; i++)
            {
                for (int n = i + 1; n < words.Count; n++)
                {
                    string newPattern = GetPatternStringFromWords(words[i], words[n]);
                    if (!allPatterns.Contains(newPattern))
                    {
                        allPatterns.Add(newPattern);
                    }
                }
            }

            // once all patterns have been determined, get a percent for all of them
            foreach (var pattern in allPatterns)
            {
                List<string> wordsWithPattern = MatchWordsToPattern(words, pattern, true);
                StringPercentContainer newPercent = new StringPercentContainer()
                {
                    String = pattern,
                    Percent = GetPercent(wordsWithPattern.Count, words.Count),
                };
                if (includeExamples)
                {
                    newPercent.ExampleList = wordsWithPattern;
                }
                patternPercents.Add(newPercent);
            }

            // return sorted patterns
            return new PatternAnalysis()
            {
                FurtherPatternStatistics = patternPercents.OrderBy(p => p.Percent).Reverse().ToList()
            };
        }

        public static double GetPercent(int count, int totalCount)
        {
            double dec = (double)count / totalCount;
            double percent = dec * 100.0;
            int decimalCount = 2;
            double percentRounded = Math.Round(percent, decimalCount);
            return percentRounded;
        }

        // often, failing words are due to words with e in index 3. Help eliminate these.
        public static bool CanHaveEIn3(List<string> possibleWords)
        {
            foreach (var word in possibleWords)
            {
                if (word.Substring(3, 1) == "e")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasDoubleLetters(string word)
        {
            string[] doubleLettersInWord = GetDoubleLettersFromWord(word);

            if (doubleLettersInWord == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool HasMultipleLetter(string word)
        {
            List<string> letters = new List<string>();

            for (int i = 0; i < word.Length; i++)
            {
                string letter = word.Substring(i, 1).ToLower();
                if (!letters.Contains(letter))
                {
                    letters.Add(letter);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasTwoMultipleLetters(string word)
        {
            List<string> letters = new List<string>();
            int multCount = 0;

            for (int i = 0; i < word.Length; i++)
            {
                string letter = word.Substring(i, 1).ToLower();
                if (!letters.Contains(letter))
                {
                    letters.Add(letter);
                }
                else
                {
                    multCount++;
                    if (multCount == 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<string> GetWordsWithMultipleLetter(List<string> words)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                if (HasMultipleLetter(word))
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        public static List<string> GetWordsWithTwoMultipleLetters(List<string> words)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                if (HasTwoMultipleLetters(word))
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        public static List<string> GetWordsWithDoubleLetters(List<string> words)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                if (HasDoubleLetters(word))
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        private static List<LetterCount> GetDoubleLetterCounts(List<string> words, SortBy sortBy = SortBy.ReverseCount)
        {
            List<string> doubleLettersList = new List<string>();
            List<LetterCount> doubleLetterCounts = new List<LetterCount>();

            // loop through all the words
            foreach (string word in words)
            {
                string[] doubleLettersInWord = GetDoubleLettersFromWord(word);

                // if doubleLettersInWord, that means it found double letters
                if (doubleLettersInWord != null)
                {
                    // loop through any sets of double letters - it's possible to have two
                    for (int i = 0; i < doubleLettersInWord.Length; i++)
                    {
                        string doubleLetters = doubleLettersInWord[i];

                        // see if the letters are already included in the list
                        // if not, add it to the lists
                        if (!doubleLettersList.Contains(doubleLetters))
                        {
                            doubleLettersList.Add(doubleLetters);
                            doubleLetterCounts.Add(new LetterCount()
                            {
                                Letter = doubleLetters,
                                Count = 1
                            });
                        }
                        // if so, find that letter count and add to it
                        else
                        {
                            doubleLetterCounts.Where(count => count.Letter
                            == doubleLetters).FirstOrDefault().Count++;
                        }
                    }
                }
            }

            // now sort counts before returning
            if (sortBy == SortBy.Count || sortBy == SortBy.ReverseCount)
            {
                doubleLetterCounts = doubleLetterCounts.OrderBy(s => s.Count).ToList();
            }
            else if ((sortBy == SortBy.Letter || sortBy == SortBy.ReverseLetter))
            {
                doubleLetterCounts = doubleLetterCounts.OrderBy(s => s.Count).ToList();
            }

            if (sortBy == SortBy.ReverseCount || sortBy == SortBy.ReverseLetter)
            {
                doubleLetterCounts.Reverse();
            }

            return doubleLetterCounts;
        }

        /// <summary>
        /// Get array of all double letters contained in a 4 letter word.
        /// If this returns null, that indicates no double letters.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static string[] GetDoubleLettersFromWord(string word)
        {
            if (word == null)
            {
                throw new ArgumentNullException("word");
            }
            else if (word.Length != 5)
            {
                throw new Exception("The word must be 5 letters long.");
            }

            string doubleLetters = null;
            string[] allDoubleLetters = null;
            string lastLetter = null;

            // loop through letters in the word to check for double letters
            for (int i = 0; i < word.Length; i++)
            {
                string thisLetter = word.Substring(i, 1).ToLower();
                // if this is the first letter, set last letter to it
                if (lastLetter == null)
                {
                    lastLetter = thisLetter;
                }
                else
                {
                    // otherwise check if this letter is the same as the last one
                    // if yes, it's a double
                    if (lastLetter == thisLetter)
                    {
                        // before setting, make sure doubleLetters is still null
                        // if so then set it from null to the new double letter string
                        if (doubleLetters == null)
                        {
                            doubleLetters = $"{lastLetter}{thisLetter}";
                        }
                        // if it's not null, that means the word has two sets of double letters
                        // (this logic assumes the word is 5 letters long.
                        else
                        {
                            allDoubleLetters = new string[] {
                                doubleLetters,
                                $"{lastLetter}{thisLetter}"
                            };
                        }


                        // set last letter back to null
                        lastLetter = null;
                    }
                    // if no, set last letter to this letter
                    else
                    {
                        lastLetter = thisLetter;
                    }
                }
            }

            // if allDoubleLetters is null, but doubleLetters is not, that
            // indicates that there is one pair of double letters.
            if (allDoubleLetters == null && doubleLetters != null)
            {
                allDoubleLetters = new string[]
                {
                    doubleLetters
                };
            }

            // if allDoubleLetters remains null, that means there are no double letters
            return allDoubleLetters;

        }

        public static bool AllWordsAreTies(List<string> words, int blanksAllowed)
        {
            if (words.Count < 2)
            {
                return false;
            }

            string patternString = GetPatternStringFromWords(words[0], words[words.Count - 1]);

            // check that the pattern doesn't have multiple blanks
            int blankCount = 0;
            for (int i = 0; i < patternString.Length; i++)
            {
                if (patternString.Substring(i, 1) == "_")
                {
                    blankCount++;
                }
            }

            if (blankCount > blanksAllowed)
            {
                return false;
            }

            // now get all words matching the pattern - if the length is the same, they are ties
            List<string> matchingWords = MatchWordsToPattern(words, patternString, true);
            if (matchingWords.Count == words.Count)
            {
                return true;
            }
            return false;
        }

        public static string GetPatternStringFromWords(string word1, string word2)
        {
            string patternString = "";
            for (int i = 0; i < word1.Length; i++)
            {
                string letter1 = word1.Substring(i, 1);
                string letter2 = word2.Substring(i, 1);

                if (letter1 == letter2)
                {
                    patternString += letter1;
                }
                else
                {
                    patternString += "_";
                }
            }
            return patternString;
        }

        public static StringPercentContainer GetWordsWithPatternNowFailing(string pattern)
        {
            List<string> newFails = new List<string>();
            List<string> matchingWords = FailAnalyzer.MatchWordsToPattern(WordSorter.GetAllWords(), pattern, true);
            List<string> failedWordList = GetFailedWords();

            foreach (var word in matchingWords)
            {
                WordleGuesses wordResult = WordSorter.GuessWordleSolution(word);
                if (!wordResult.DidWin && !failedWordList.Contains(word))
                {
                    newFails.Add(word);
                }
            }

            StringPercentContainer result = new StringPercentContainer()
            {
                String = "now failing",
                Percent = (double)newFails.Count,
                //Percent = GetPercent(newFails.Count, matchingWords.Count),
                ExampleList = newFails
            };

            return result;
        }

        /// <summary>
        /// Gets a list of all words that match a specified pattern. Must adhere to specific format.
        /// </summary>
        /// <param name="words"></param>
        /// <param name="pattern"></param>
        /// <param name="isInclusive"></param>
        /// <returns></returns>
        public static List<string> MatchWordsToPattern(List<string> words, string pattern, bool isInclusive)
        {
            List<string> matching = new List<string>();
            List<string> nonMatching = new List<string>();
            string[] patternArray = GetPattern(pattern);

            foreach (var word in words)
            {
                bool doesMatch = DoesWordMatchPattern(patternArray, word);
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

        public static bool DoesWordMatchPattern(string pattern, string word)
        {
            string[] patternArray = GetPattern(pattern);
            return DoesWordMatchPattern(patternArray, word);
        }

        public static bool DoesWordMatchPattern(string[] patternArray, string word)
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

            return doesMatch;
        }


        /// <summary>
        /// Takes a string containing a pattern and turns it into an array. Each _ represents an unknown letter.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string[] GetPattern(string word)
        {
            string[] pattern = new string[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                string character = word.Substring(i, 1).ToLower();
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


        /// <summary>
        /// Gets the most common letters that show up in each position in a word.
        /// </summary>
        /// <returns></returns>
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

        public static List<string> GetAllWordsWithoutPatterns(string patternsString, List<string> words)
        {


            // divide up patterns
            string[] patterns = patternsString.Split(",");

            // go through all words and add to final list if they don't match any patters
            List<string> finalList = new List<string>();
            foreach (var word in words)
            {
                bool matchesPattern = false;
                for (int i = 0; i < patterns.Length; i++)
                {
                    string pattern = patterns[i];
                    if (DoesWordMatchPattern(pattern, word))
                    {
                        matchesPattern = true;
                        break;
                    }
                }
                if (!matchesPattern)
                {
                    finalList.Add(word);
                }
            }

            return finalList;
        }



    }

    public enum SortBy
    {
        Letter,
        Count,
        ReverseLetter,
        ReverseCount
    }
}
