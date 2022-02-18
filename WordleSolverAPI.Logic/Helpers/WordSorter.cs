﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordleSolverAPI.Logic.Enums;
using WordleSolverAPI.Logic.Models;

namespace WordleSolverAPI.Logic
{
    public static class WordSorter
    {
        public static string[] allLetters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                                                            "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
                                                            "w", "x", "y", "z"};
        public static WordleGuesses GuessWordleSolution(string correctAnswer)
        {
            DateTime beginningTime = DateTime.Now;
            StartMode startMode = StartMode.Strict;
            SolveMode solveMode = SolveMode.Normal;

            Random random = new Random();
            WordleGuesses endResult = new WordleGuesses();
            endResult.CorrectAnswer = correctAnswer;
            bool hasError = false;
            bool endsWithS = false;

            // first get all possible words
            List<string> possibleWords = GetAllWords();
            List<string> allWords = GetAllWords();
            // currently, 75% of failing words end with the letter s
            List<string> failingWords = FailAnalyzer.GetFailedWords();

            List<LetterWrongPositions> letterWrongPositions = new List<LetterWrongPositions>();
            List<LetterPosition> correctPositions = new List<LetterPosition>();

            // guess up to 6 times to get correct answer
            for (int i = 0; i < 6; i++)
            {
                // figure out the most common letter in possible answers - TODO improve this part?
                string[] mostCommonLetters;
                if (startMode == StartMode.Guess)
                {
                    mostCommonLetters = GetMostCommonLetters(possibleWords, 3); // 3 is the sweet spot, it seems
                }
                else
                {
                    mostCommonLetters = GetMostCommonLetters(possibleWords, 5); // 3 is the sweet spot, it seems
                }

                // add possible word count to solution if halfway through
                if (i == 3)
                {
                    endResult.RemainingPossibleAt3 = possibleWords.Count;
                }
                else if (i == 1) // check if it ends with S! right after first guess!
                {
                    // NOTE: with current logic, the first guess should end with s.
                    WordGuess lastGuess = endResult.Guesses[0];
                    if (lastGuess.Result[lastGuess.Result.Length - 1].Status == LetterStatus.Correct)
                    {
                        endsWithS = true;
                    }
                    else
                    {
                        endsWithS = false;
                    }
                }

                string newGuessWord;
                if (i == 0)
                {

                    // the below one tends to be slightly more accurate while the last one is much faster
                    if (startMode == StartMode.Guess)
                    {
                        newGuessWord = ChooseRandomWord(GetListOfWordsWithLetters(mostCommonLetters, possibleWords), random);
                    }
                    else
                    {
                        newGuessWord = GetInitialGuessWord(GetListOfWordsWithLetters(mostCommonLetters, possibleWords),
                        possibleWords, mostCommonLetters);
                    }


                }
                else
                {
                    //solveMode = SolveMode.Normal;
                    //// change the mode under certain circumstances
                    //if ((i == 3 && possibleWords.Count > 15) ||
                    //    (i > 3 && possibleWords.Count > 5 - i))
                    //{
                    //    //solveMode = SolveMode.NewTurbo;
                    //    //solveMode = SolveMode.Letters;
                    //    solveMode = SolveMode.Pattern;
                    //}

                    WordGuess lastGuess = endResult.Guesses[i - 1];
                    // modify wrong position possibilities - add correct letters
                    List<LetterPosition> newCorrectPositions = ModifyCorrectPositionsGetNew(correctPositions, lastGuess);
                    letterWrongPositions = AddToWrongPositionLetters(letterWrongPositions, lastGuess, newCorrectPositions);


                    // check first if the word is likely to be on the fail list or not before using special mode
                    if (solveMode == SolveMode.Pattern)
                    {
                        int failCount = FailAnalyzer.GetPossibleFailedWords(possibleWords, failingWords).Count;
                        double failLikelihood = FailAnalyzer.GetPercent(failCount, possibleWords.Count);

                        if (failLikelihood < 50)
                        {
                            solveMode = SolveMode.Normal;
                        }
                    }

                    // guess differently depending on solving mode
                    if (solveMode == SolveMode.Normal)
                    {
                        newGuessWord = GetGuessWordByLetterDistribution(possibleWords, lastGuess, letterWrongPositions);
                        if (correctAnswer != "error" && newGuessWord == "error")
                        {
                            hasError = true;
                        }
                    }
                    else if (solveMode == SolveMode.Pattern)
                    {


                        List<string> concernedWords = possibleWords;
                        bool allWordsAreTies = FailAnalyzer.AllWordsAreTies(concernedWords, 1);

                        if (allWordsAreTies)
                        {
                            concernedWords = GetMostLikelyWordsByBlankPosition(concernedWords);
                        }
                        else
                        {
                            concernedWords = GetMostLikelyWordsByPatterns(concernedWords);
                        }

                        if (allWordsAreTies)
                        {
                            newGuessWord = GetGuessWordByCompleteLetterDistribution(concernedWords, allWords, letterWrongPositions);
                        }
                        else
                        {
                            newGuessWord = GetGuessWordByLetterDistribution(concernedWords, lastGuess, letterWrongPositions);
                        }
                        //newGuessWord = GetGuessWordByLetterDistribution(concernedWords, lastGuess);
                        if (correctAnswer != "error" && newGuessWord == "error")
                        {
                            hasError = true;
                        }

                    }
                    else if (solveMode == SolveMode.Letters)
                    {
                        string matchString = "";
                        for (int n = 0; n < 5; n++)
                        {
                            string newLetter = GetMostCommonLetterInPosition(possibleWords, n);
                            matchString += newLetter;
                        }

                        // now get a list of all the words with a match percent
                        //List<StringPercentContainer> matchPercents = GetWordMatchPercentsWithPosition(matchString, possibleWords);
                        List<StringPercentContainer> commonLetterMatchPercents = GetLettersMatchPercents(mostCommonLetters, possibleWords);
                        //StringPercentContainer wMatchP = matchPercents[0];
                        StringPercentContainer lMatchP = commonLetterMatchPercents[0];
                        //string newG = "";
                        //if (wMatchP.Percent > lMatchP.Percent)
                        //{
                        //    newG = wMatchP.String;
                        //}
                        //else
                        //{
                        //    newG = lMatchP.String;
                        //}

                        newGuessWord = lMatchP.String;
                        if (correctAnswer != "error" && newGuessWord == "error")
                        {
                            hasError = true;
                        }

                    }
                    else if (solveMode == SolveMode.NewTurbo)
                    {
                        int iReminder = i;
                        string answerReminder = correctAnswer;

                        // count possible words with these common patterns in failed words
                        int s5Count = FailAnalyzer.MatchWordsToPattern(possibleWords, "____s", true).Count;
                        int e4Count = FailAnalyzer.MatchWordsToPattern(possibleWords, "___e_", true).Count;
                        int e4s5Count = FailAnalyzer.MatchWordsToPattern(possibleWords, "___es", true).Count;
                        int r5Count = FailAnalyzer.MatchWordsToPattern(possibleWords, "____r", true).Count;
                        int y5Count = FailAnalyzer.MatchWordsToPattern(possibleWords, "____y", true).Count;

                        // NOTE consider using GetInitialGuessWord or methods in there to decide
                        newGuessWord = GetGuessWordByLetterDistribution(possibleWords, lastGuess, letterWrongPositions);
                    }
                    else if (solveMode == SolveMode.Turbo) // This is where failing words will become accounted for
                    {
                        List<string> concernedWords = possibleWords;
                        bool isEd = false;
                        bool isEr = false;
                        //bool isY = false;

                        // if the word does not end in s, then do some further narrowing down
                        if (!endsWithS)
                        {
                            bool canHaveE = FailAnalyzer.CanHaveEIn3(possibleWords);
                            if (canHaveE)
                            {
                                int eIn3Count = concernedWords.Where(word =>
                                    word.Substring(3, 1) == "e").ToList().Count;
                                double chanceOfEIn3 = FailAnalyzer.GetPercent(eIn3Count, concernedWords.Count);

                                if (chanceOfEIn3 > 50)
                                {
                                    if (chanceOfEIn3 < 100)
                                    {
                                        concernedWords = concernedWords.Where(word =>
                                        word.Substring(3, 1) == "e").ToList();
                                    }

                                    //43.35% of failing words without s at the end are ending in ed
                                    // 18.62% are ending with er
                                    // that's 61.97% chance it ends in one of these two
                                    int edCount = concernedWords.Where(word =>
                                    word.Substring(3, 2) == "ed").ToList().Count;

                                    if (edCount > 0 && edCount < concernedWords.Count)
                                    {
                                        double chanceOfEd = FailAnalyzer.GetPercent(edCount, concernedWords.Count);

                                        if (chanceOfEd == 100)
                                        {
                                            isEd = true;
                                        }
                                        else if (chanceOfEd > 50)
                                        {
                                            concernedWords = concernedWords.Where(word =>
                                        word.Substring(3, 2) == "ed").ToList();
                                        }
                                        else
                                        {
                                            concernedWords = concernedWords.Where(word =>
                                        word.Substring(3, 2) != "ed").ToList();
                                        }
                                    }

                                    // check er
                                    if (edCount == 0)
                                    {
                                        int erCount = concernedWords.Where(word =>
                                     word.Substring(3, 2) == "er").ToList().Count;

                                        if (erCount == concernedWords.Count)
                                        {
                                            isEr = true;
                                        }

                                        if (erCount > 0 && erCount < concernedWords.Count)
                                        {
                                            double chanceOfEr = FailAnalyzer.GetPercent(erCount, concernedWords.Count);
                                            if (chanceOfEr > 50)
                                            {
                                                concernedWords = concernedWords.Where(word =>
                                            word.Substring(3, 2) == "er").ToList();
                                            }
                                            else
                                            {
                                                concernedWords = concernedWords.Where(word =>
                                            word.Substring(3, 2) != "er").ToList();
                                            }
                                        }
                                    }


                                }
                                else if (chanceOfEIn3 > 0 && chanceOfEIn3 <= 50)
                                {
                                    concernedWords = concernedWords.Where(word =>
                                    word.Substring(3, 1) != "e").ToList();
                                }
                            }
                        }

                        //if (!isEd && !isEr && !endsWithS)
                        //{
                        //    int yCount = concernedWords.Where(word =>
                        //            word.Substring(4, 1) == "y").ToList().Count;

                        //    if (yCount == concernedWords.Count)
                        //    {
                        //        isY = true;
                        //    }
                        //    else if (yCount > 0)
                        //    {
                        //        double chanceOfY = FailAnalyzer.GetPercent(yCount, concernedWords.Count);

                        //        if (chanceOfY > 50)
                        //        {
                        //            concernedWords = concernedWords.Where(word =>
                        //        word.Substring(4, 1) == "y").ToList();
                        //            isY = true;
                        //        }
                        //    }
                        //}

                        // if it is er or ed, check for other stuff
                        if (isEd || isEr)
                        {
                            // of everthing that applies to these, almost half have the letter a as a second letter - followed by i and 0
                            // these will apply to 92.63% of ed words and 94.28% of er words
                            string[] checkOrder;
                            if (isEd)
                            {
                                checkOrder = new string[] { "a", "o", "i" };
                            }
                            else
                            {
                                checkOrder = new string[] { "a", "i", "o" };
                            }

                            string checkString = null;
                            List<string> wordsThatApply = null;
                            for (int n = 0; n < checkOrder.Length; n++)
                            {
                                checkString = checkOrder[n];
                                wordsThatApply = concernedWords.Where(word =>
                                word.Substring(1, 1) == checkString).ToList();
                                if (wordsThatApply.Count > 0 &&
                                    wordsThatApply.Count < concernedWords.Count)
                                {
                                    break;
                                }
                                // if nothig comes up, set back to null
                                if (n == checkOrder.Length - 1)
                                {
                                    checkString = null;
                                    wordsThatApply = null;
                                }
                            }

                            // if words that apply isn't null, test out that list of words
                            if (wordsThatApply != null)
                            {
                                concernedWords = wordsThatApply;
                            }
                        }
                        //else if (isY)
                        //{
                        //    // 73.33% of failing words ending in y have double letters
                        //    int totalWords = concernedWords.Count;
                        //    List<string> wordsWithDoubles = FailAnalyzer.GetWordsWithDoubleLetters(concernedWords);
                        //    double yDoublePercent = FailAnalyzer.GetPercent(wordsWithDoubles.Count, totalWords);
                        //    if (yDoublePercent != 0 && yDoublePercent < 100)
                        //    {
                        //        if (yDoublePercent >= 50)
                        //        {
                        //            concernedWords = wordsWithDoubles;
                        //        }
                        //    }
                        //}

                        // only 12% of failing words ending in s have double letters
                        newGuessWord = GetGuessWordByLetterDistribution(concernedWords, lastGuess, letterWrongPositions);
                        //if (concernedWords.Count > 1 && FailAnalyzer.AllWordsAreTies(concernedWords, 1))
                        //{
                        //    int failedCount = concernedWords.Where(word => failingWords.Contains(word)).ToList().Count;
                        //    double failedPercent = FailAnalyzer.GetPercent(failedCount, possibleWords.Count);
                        //    if (failedPercent >= 50)
                        //    {
                        //        newGuessWord = GetGuessWordByCompleteLetterDistribution(concernedWords, failingWords);
                        //    }
                        //    else
                        //    {
                        //        newGuessWord = GetGuessWordByCompleteLetterDistribution(concernedWords, GetAllWords());
                        //    }

                        //}
                        //else
                        //{
                        //    newGuessWord = GetGuessWordByLetterDistribution(concernedWords, lastGuess);
                        //}

                        if (correctAnswer != "error" && newGuessWord == "error")
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        throw new Exception(
                            "That solving mode is not accounted for " +
                            "with any logic in order to guess a new word.");
                    }

                    //List<LetterPosition> lastCorrectLetters = GetCorrectLetters(endResult.Guesses[i - 1].Result);
                    //string mostCommonLetter = GetMostCommonLetterNotInPosition(possibleWords, lastCorrectLetters);
                    //newGuessWord = ChooseRandomWord(GetListOfWordsWithLetter(mostCommonLetter, possibleWords), random);
                }

                // make guess, get results
                WordGuess newGuess = GetGuessResults(newGuessWord, correctAnswer);
                newGuess.GuessNumber = i + 1;
                endResult.Guesses.Add(newGuess);

                // if it was correct, set winner
                if (newGuess.IsCorrect)
                {
                    endResult.DidWin = true;
                    break;
                }

                // otherwise, eliminate possibilities based on guess result
                possibleWords = EliminateBasedOnGuess(possibleWords, newGuess);

                // now it will loop until it gets a correct answer
            }
            DateTime endTime = DateTime.Now;
            endResult.TimeToSolve = endTime - beginningTime;
            endResult.GuessCount = endResult.Guesses.Count;
            endResult.IsFinished = true;
            endResult.HasError = hasError;
            return endResult;
        }

        public static int CountWordsWithPattern(string pattern, bool checkFailing)
        {
            List<string> matchingWords;
            if (!checkFailing)
            {
                matchingWords = FailAnalyzer.MatchWordsToPattern(GetAllWords(), pattern, true);
            }
            else
            {
                matchingWords = FailAnalyzer.MatchWordsToPattern(FailAnalyzer.GetFailedWords(), pattern, true);
            }

            return (matchingWords.Count);
        }

        public static List<StringPercentContainer> GetPassFailRatesForWordsWithPattern(string pattern, bool checkFailing)
        {
            List<StringPercentContainer> results = new List<StringPercentContainer>();
            List<string> matchingWords;
            if (!checkFailing)
            {
                matchingWords = FailAnalyzer.MatchWordsToPattern(GetAllWords(), pattern, true);
            }
            else
            {
                matchingWords = FailAnalyzer.MatchWordsToPattern(FailAnalyzer.GetFailedWords(), pattern, true); ;
            }
            int passingCount = 0;
            int failingCount = 0;

            foreach (var word in matchingWords)
            {
                WordleGuesses result = GuessWordleSolution(word);
                if (result.DidWin)
                {
                    passingCount++;
                }
                else
                {
                    failingCount++;
                }
            }

            int total = passingCount + failingCount;
            results.Add(new StringPercentContainer()
            {
                String = "passing",
                Percent = FailAnalyzer.GetPercent(passingCount, total)
            });
            results.Add(new StringPercentContainer()
            {
                String = "failing",
                Percent = FailAnalyzer.GetPercent(failingCount, total)
            });

            return results;
        }

        public static SolverStatistics GetStatisticsForCommonWordleWords(int startIndex, int howMany)
        {
            return GetStatistics(howMany, true, true, startIndex);
        }

        public static List<StringPercentContainer> GetLettersMatchPercents(string[] letters, List<string> allWords)
        {
            List<StringPercentContainer> matchPercents = new List<StringPercentContainer>();
            foreach (var word in allWords)
            {
                string editedWord = word;
                int matchCount = 0;
                for (int n = 0; n < letters.Length; n++)
                {
                    string letter = letters[n];
                    for (int i = 0; i < editedWord.Length; i++)
                    {
                        if (editedWord.Substring(i, 1) == letter)
                        {
                            matchCount++;
                            editedWord = editedWord.Remove(i, 1);
                            break;
                        }
                    }
                }

                StringPercentContainer newContainer = new StringPercentContainer()
                {
                    String = word,
                    Percent = FailAnalyzer.GetPercent(matchCount, 5)
                };
                matchPercents.Add(newContainer);
            }

            matchPercents = matchPercents.OrderBy(p => p.Percent).Reverse().ToList();
            return matchPercents;
        }

        public static List<StringPercentContainer> GetWordMatchPercentsWithPosition(string matchWord, List<string> allWords)
        {
            List<StringPercentContainer> matchPercents = new List<StringPercentContainer>();
            foreach (var word in allWords)
            {
                int matchCount = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (word.Substring(i, 1) == matchWord.Substring(i, 1))
                    {
                        matchCount++;
                    }
                }
                StringPercentContainer newContainer = new StringPercentContainer()
                {
                    String = word,
                    Percent = FailAnalyzer.GetPercent(matchCount, 5)
                };
                matchPercents.Add(newContainer);
            }

            matchPercents = matchPercents.OrderBy(p => p.Percent).Reverse().ToList();
            return matchPercents;
        }

        public static List<string> GetMostLikelyWordsByPatterns(List<string> words)
        {
            // make a copy of the list
            List<string> wordsCopy = new List<string>();
            words.ForEach(word => wordsCopy.Add(word));

            PatternAnalysis analysis;
            StringPercentContainer pContainer;
            string pattern = null;
            double percent = 0;
            int blanksInPattern = 0;

            do
            {
                analysis = FailAnalyzer.GetPatternsByPercents(wordsCopy);
                pContainer = GetPatternNotAt100Percent(analysis);
                pattern = null;
                percent = 0;
                if (pContainer != null)
                {
                    pattern = pContainer.String;
                    percent = pContainer.Percent;
                }
                blanksInPattern = CountBlanksInPatter(pattern);

                if (percent > 0)
                {
                    wordsCopy = FailAnalyzer.MatchWordsToPattern(wordsCopy, pattern, true);
                }
                else
                {
                    break;
                }
            } while (blanksInPattern > 0);

            return wordsCopy;
        }

        public static List<string> GetMostLikelyWordsByBlankPosition(List<string> words)
        {
            PatternAnalysis analysis;
            StringPercentContainer pContainer;
            analysis = FailAnalyzer.GetPatternsByPercents(words);
            pContainer = analysis.FurtherPatternStatistics[0];
            string[] patternArray = FailAnalyzer.GetPattern(pContainer.String);

            List<int> blankPositions = new List<int>();
            for (int i = 0; i < patternArray.Length; i++)
            {
                if (patternArray[i] == null)
                {
                    blankPositions.Add(i);
                }
            }
            List<PositionLetterFrequencies> frequencies = GetLetterFrequenciesInUnknownPositions(words, blankPositions, allLetters);

            // loop through frequencies until able to make a list of words
            List<string> finalList = new List<string>();
            foreach (var f in frequencies)
            {
                int index = 0;
                int position = f.Position;
                List<string> listToAdd = new List<string>();
                do
                {
                    string letter = f.LetterCounts[index].Letter;
                    foreach (var word in words)
                    {
                        if (word.Substring(position, 1) == letter)
                        {
                            listToAdd.Add(word);
                        }
                    }
                } while (listToAdd.Count == 0 && index < f.LetterCounts.Count);

                // add those words to final list
                foreach (var item in listToAdd)
                {
                    finalList.Add(item);
                }
            }

            return finalList;

        }

        public static StringPercentContainer GetPatternNotAt100Percent(PatternAnalysis analysis)
        {
            if (analysis.FurtherPatternStatistics == null ||
                analysis.FurtherPatternStatistics.Count == 0)
            {
                return null;
            }
            else
            {
                int count = 0;
                do
                {
                    if (analysis.FurtherPatternStatistics[count].Percent != 100)
                    {
                        return analysis.FurtherPatternStatistics[count];
                    }
                    else
                    {
                        count++;
                    }
                } while (count < analysis.FurtherPatternStatistics.Count);

                return null;
            }
        }

        private static int CountBlanksInPatter(string pattern)
        {
            if (pattern == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern.Substring(i, 1) == "_")
                {
                    count++;
                }
            }
            return count;
        }

        public static SolverStatistics GetStatistics(int timesToRun, bool allowWordsEndingInS = true, bool isForCommon = false, int startIndex = 0)
        {
            Random random = new Random();
            List<string> allFailingWords = FailAnalyzer.GetFailedWords();
            List<string> allWords;

            if (isForCommon)
            {
                allWords = GetAllWords(WordListType.WordleCommon);
            }
            else
            {
                allWords = GetAllWords();
            }

            if (!allowWordsEndingInS)
            {
                allFailingWords = allFailingWords.Where(word =>
                word.Substring(4, 1) != "s").ToList();
                allWords = allWords.Where(word =>
                 word.Substring(4, 1) != "s").ToList();
            }

            // make sure they're not requesting to run it more times than possible - fix if so
            if (timesToRun > (allWords.Count - startIndex + 1))
            {
                timesToRun = allWords.Count;
            }

            int correctAnswerCount = 0;
            int guessesCount = 0;
            int remainingAt3SuccessCount = 0;
            int remainingAt3FailCount = 0;
            double millisecondsCount = 0;
            List<string> failedWords = new List<string>();
            List<string> errorWords = new List<string>();
            List<string> failedWordsNotOnFailList = new List<string>();
            List<string> newSuccessWords = new List<string>();

            for (int i = startIndex; i < (startIndex + timesToRun); i++)
            {
                string correctAnswer;
                if (isForCommon)
                {
                    correctAnswer = allWords[i];
                }
                else
                {
                    correctAnswer = WordSorter.ChooseRandomWord(allWords, random);
                }

                WordleGuesses result = WordSorter.GuessWordleSolution(correctAnswer);

                if (result.DidWin == true)
                {
                    correctAnswerCount++;
                    guessesCount += result.GuessCount;
                    remainingAt3SuccessCount += result.RemainingPossibleAt3;
                    if (allFailingWords.Contains(result.CorrectAnswer))
                    {
                        newSuccessWords.Add(result.CorrectAnswer);
                    }
                }
                else
                {
                    failedWords.Add(result.CorrectAnswer);
                    remainingAt3FailCount += result.RemainingPossibleAt3;
                    if (!allFailingWords.Contains(result.CorrectAnswer))
                    {
                        failedWordsNotOnFailList.Add(result.CorrectAnswer);
                    }
                }

                if (result.HasError)
                {
                    errorWords.Add(result.CorrectAnswer);
                }

                //guessesCount += result.GuessCount;
                millisecondsCount += result.TimeToSolve.Milliseconds;

                // remove word from possible words and failing words
                allWords = allWords.Where(word => word != correctAnswer).ToList();
                allFailingWords = allFailingWords.Where(word => word != correctAnswer).ToList();
            }

            failedWords.Sort();
            failedWordsNotOnFailList.Sort();

            return new SolverStatistics()
            {
                CorrectAnswerRate = (double)correctAnswerCount / timesToRun,
                AverageGuessesToWin = (double)guessesCount / timesToRun,
                AverageMilliseconds = millisecondsCount / timesToRun,
                AverageRemainingAt3Success = (double)remainingAt3SuccessCount / (timesToRun - failedWords.Count),
                AverageRemainingAt3Fail = (double)remainingAt3FailCount / failedWords.Count,
                FailedWords = failedWords,
                ErrorWords = errorWords,
                FailedWordsNotOnFailList = failedWordsNotOnFailList,
                NewSuccessWords = newSuccessWords,
                CorrectAnswerRateNoErrors = (double)(correctAnswerCount - errorWords.Count) /
                    (timesToRun - errorWords.Count)
            };
        }

        private static List<string> EliminateBasedOnGuess(List<string> possibleWords, WordGuess guess)
        {
            // loop three times, once for correct, once for incorrect, 2nd time for wrong positions...
            // during first time, keep track of correct letters so they're not counted for wrong position possibilities
            List<LetterPosition> correctLetters = GetCorrectLetters(guess.Result);
            List<LetterPosition> wrongPositionLetters = GetWrongPositionLetters(guess.Result);
            List<LetterPosition> incorrectLetters = GetIncorrectLetters(guess.Result);

            // eliminate words without correct letters in correct positions
            foreach (var correctLetter in correctLetters)
            {
                possibleWords = GetWordsWithLetterInPosition(possibleWords, correctLetter.Letter, correctLetter.Position);
            }

            // eliminate words that have a letter in a wrong position or incorrect letter in position
            // this is because if it's the same as a wrong position letter, you don't want to eliminate all words that have that letter,
            // you only want to eliminate words with that letter in that position
            foreach (var incorrectLetter in incorrectLetters)
            {
                bool isContainedInWrongPositions = false;
                foreach (var wrongPosLetter in wrongPositionLetters)
                {
                    if (wrongPosLetter.Letter == incorrectLetter.Letter)
                    {
                        isContainedInWrongPositions = true;
                        break;
                    }
                }
                if (isContainedInWrongPositions)
                {
                    possibleWords = GetWordsWithNoLetterInPosition(possibleWords, incorrectLetter.Letter, incorrectLetter.Position);
                }
                else
                {
                    possibleWords = GetWordsWithNoLetter(possibleWords, incorrectLetter.Letter, correctLetters);
                }
            }

            foreach (var wrongPosLetter in wrongPositionLetters)
            {
                possibleWords = GetWordsWithNoLetterInPosition(possibleWords, wrongPosLetter.Letter, wrongPosLetter.Position);
            }

            // now figure out which positions are possible for the wrong position letters and eliminate words without the wrong
            // position letters in one of the remaining positions
            List<string> possibleWordsFinal = new List<string>();
            foreach (var word in possibleWords)
            {
                bool canAdd = true;

                List<LetterPosition> correctLettersCopy = new List<LetterPosition>();
                foreach (var correctLetter in correctLetters) { correctLettersCopy.Add(correctLetter); };

                string wordWithNoEliminatedLetters = GetWordWithoutCorrectLetters(word, correctLettersCopy);
                foreach (var wrongPosLetter in wrongPositionLetters)
                {
                    if (!wordWithNoEliminatedLetters.Contains(wrongPosLetter.Letter))
                    {
                        canAdd = false;
                        break;
                    }
                    else
                    {
                        int splicePos = 0;
                        for (int i = 0; i < wordWithNoEliminatedLetters.Length; i++)
                        {
                            if (wordWithNoEliminatedLetters.Substring(i, 1) == wrongPosLetter.Letter)
                            {
                                splicePos = i;
                                break;
                            }
                        }
                        wordWithNoEliminatedLetters = wordWithNoEliminatedLetters.Remove(splicePos, 1);
                        //correctLettersCopy.Add(wrongPosLetter);
                        //wordWithNoEliminatedLetters = GetWordWithoutCorrectLetters(wordWithNoEliminatedLetters, correctLettersCopy);
                    }
                }

                if (canAdd)
                {
                    possibleWordsFinal.Add(word);
                }
            }

            return possibleWordsFinal;
        }

        public static List<LetterPosition> ModifyCorrectPositionsGetNew(List<LetterPosition> correctPos, WordGuess lastGuess)
        {
            List<LetterPosition> newPositions = new List<LetterPosition>();
            foreach (var pos in lastGuess.Result)
            {
                bool isInList = false;
                foreach (var c in correctPos)
                {
                    if (c.Letter == pos.Letter &&
                        c.Position == pos.Position)
                    {
                        isInList = true;
                        break;
                    }
                }

                if (!isInList)
                {
                    if (pos.Status == LetterStatus.Correct)
                    {
                        newPositions.Add(pos);
                        correctPos.Add(pos);
                    }
                }

            }
            return newPositions;
        }

        public static double GetPercentOfWordsWithPattern(string pattern, bool checkFailing, bool includeSEnding = true)
        {
            List<string> wordsThatApply = new List<string>();
            List<string> allWords;
            if (checkFailing)
            {
                allWords = FailAnalyzer.GetFailedWords();
            }
            else
            {
                allWords = GetAllWords();
            }

            if (!includeSEnding)
            {
                allWords = allWords.Where(word => word.Substring(4, 1) != "s").ToList();
            }

            wordsThatApply = FailAnalyzer.MatchWordsToPattern(allWords, pattern, true);
            return FailAnalyzer.GetPercent(wordsThatApply.Count, allWords.Count);
        }

        public static PatternAnalysis AnalyzePattern(string pattern, bool checkFailing, bool includeSEnding = true)
        {
            List<string> allWords;
            if (checkFailing)
            {
                allWords = FailAnalyzer.GetFailedWords();
            }
            else
            {
                allWords = GetAllWords();
            }

            if (!includeSEnding)
            {
                allWords = allWords.Where(word => word.Substring(4, 1) != "s").ToList();
            }

            List<string> wordsWithPattern = GetWordsWithPattern(pattern, checkFailing, includeSEnding);
            PatternAnalysis analysis = FailAnalyzer.GetPatternsByPercents(wordsWithPattern);
            analysis.FurtherPatternStatistics = analysis.FurtherPatternStatistics.Where(c => c.String != pattern).ToList();
            analysis.StartingPattern = pattern;
            analysis.PercentOfWordsWithStartingPattern = FailAnalyzer.GetPercent(wordsWithPattern.Count, allWords.Count);
            analysis.WordsWithStartingPattern = wordsWithPattern;

            return analysis;
        }

        public static PatternAnalysis AnalyzeNonPatterns(string patternsString, bool checkFailing, bool includeSEnding)
        {
            List<string> words;
            if (checkFailing)
            {
                words = FailAnalyzer.GetFailedWords();
            }
            else
            {
                words = WordSorter.GetAllWords();
            }

            if (!includeSEnding)
            {
                words = words.Where(word => word.Substring(4, 1) != "s").ToList();
            }

            List<string> wordsWithoutPatterns = FailAnalyzer.GetAllWordsWithoutPatterns(patternsString, words);
            PatternAnalysis analysis = FailAnalyzer.GetPatternsByPercents(wordsWithoutPatterns);
            analysis.FurtherPatternStatistics = analysis.FurtherPatternStatistics.Where(c => c.String != "_____").ToList();
            analysis.StartingPattern = patternsString;
            analysis.PercentOfWordsWithStartingPattern = FailAnalyzer.GetPercent(wordsWithoutPatterns.Count, words.Count);
            analysis.WordsWithStartingPattern = wordsWithoutPatterns;

            return analysis;
        }

        public static List<string> GetWordsWithPattern(string pattern, bool checkFailing, bool includeSEnding = true)
        {
            List<string> wordsThatApply = new List<string>();
            List<string> allWords;
            if (checkFailing)
            {
                allWords = FailAnalyzer.GetFailedWords();
            }
            else
            {
                allWords = GetAllWords();
            }

            if (!includeSEnding)
            {
                allWords = allWords.Where(word => word.Substring(4, 1) != "s").ToList();
            }

            return FailAnalyzer.MatchWordsToPattern(allWords, pattern, true);
        }

        private static string FindMostProbableWord(List<string> words)
        {
            List<WordProbability> scores = GetWordProbabilityScores(words).OrderBy(s => s.Score).Reverse().ToList();
            return scores[0].Word;
        }

        private static string FindMostProbableWordCompareAll(List<string> words, List<string> allWords)
        {
            List<WordProbability> scores = GetWordProbabilityScoresCompareAll(words, allWords).OrderBy(s => s.Score).Reverse().ToList();
            return scores[0].Word;
        }

        private static string GetProbableWordFromUnknownLetters(LetterPosition[] pattern, List<string> words)
        {
            if (words.Count == 0) // TODO fix cases where this happens - figure out why it happens sometimes
            {
                return "error";
            }

            List<int> unknownPositions = new List<int>();
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i].Status == LetterStatus.Incorrect)
                {
                    unknownPositions.Add(i);
                }
            }

            List<WordProbability> scores = GetWordProbabilityScoresByUnknownLetters(unknownPositions, words)
                .OrderBy(s => s.Score).Reverse().ToList();

            return scores[0].Word;
        }

        private static List<WordProbability> GetWordProbabilityScores(List<string> words)
        {
            List<WordProbability> scores = new List<WordProbability>();
            foreach (var word in words)
            {
                double score = CalculateWordProbabilityScore(word, words);
                scores.Add(new WordProbability()
                {
                    Word = word,
                    Score = score
                });
            }
            return scores;
        }

        private static List<WordProbability> GetWordProbabilityScoresByUnknownLetters(List<int> positions, List<string> words)
        {
            List<WordProbability> scores = new List<WordProbability>();
            foreach (var word in words)
            {
                double score = CalculateWordProbabilityScoreByUnknownLetters(positions, word, words);
                scores.Add(new WordProbability()
                {
                    Word = word,
                    Score = score
                });
            }
            return scores;
        }

        private static List<WordProbability> GetWordProbabilityScoresCompareAll(List<string> words, List<string> allWords)
        {
            List<WordProbability> scores = new List<WordProbability>();
            foreach (var word in words)
            {
                double score = CalculateWordProbabilityScore(word, allWords);
                scores.Add(new WordProbability()
                {
                    Word = word,
                    Score = score
                });
            }
            return scores;
        }

        private static double CalculateWordProbabilityScoreByUnknownLetters(List<int> positions, string word, List<string> words)
        {
            double total = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                string letter = word.Substring(positions[i], 1);
                total += CalculateLetterProbability(words, letter, positions[i]);
            }

            return total;
        }

        private static double CalculateWordProbabilityScore(string word, List<string> words)
        {
            double total = 0;
            for (int i = 0; i < word.Length; i++)
            {
                string letter = word.Substring(i, 1);
                total += CalculateLetterProbability(words, letter, i);
            }

            return total;
        }

        private static double CalculateLetterProbability(List<string> words, string letter, int position)
        {
            int letterCount = CountWordsWithLetterInPosition(words, letter, position);
            int totalWords = words.Count;

            return ((double)letterCount / (double)totalWords);
        }

        private static string GetWordWithoutCorrectLetters(string word, List<LetterPosition> correctLetters)
        {
            string newWord = "";
            for (int i = 0; i < word.Length; i++)
            {
                bool doesApply = false;
                foreach (var letter in correctLetters)
                {
                    if (letter.Position == i && letter.Letter == word.Substring(i, 1))
                    {
                        doesApply = true;
                        break;
                    }
                }

                if (!doesApply)
                {
                    newWord += word.Substring(i, 1);
                }
            }
            return newWord;
        }

        private static List<LetterPosition> GetCorrectLetters(LetterPosition[] letters)
        {
            List<LetterPosition> correctLetters = new List<LetterPosition>();
            foreach (var letter in letters)
            {
                if (letter.Status == LetterStatus.Correct)
                {
                    correctLetters.Add(letter);
                }
            }
            return correctLetters;
        }

        private static List<LetterPosition> GetWrongPositionLetters(LetterPosition[] letters)
        {
            List<LetterPosition> wrongPositionLetters = new List<LetterPosition>();
            foreach (var letter in letters)
            {
                if (letter.Status == LetterStatus.WrongPosition)
                {
                    wrongPositionLetters.Add(letter);
                }
            }
            return wrongPositionLetters;
        }

        private static List<LetterPosition> GetIncorrectLetters(LetterPosition[] letters)
        {
            List<LetterPosition> wrongLetters = new List<LetterPosition>();
            foreach (var letter in letters)
            {
                if (letter.Status == LetterStatus.Incorrect)
                {
                    wrongLetters.Add(letter);
                }
            }
            return wrongLetters;
        }
        private static int CountWordsWithLetterInPosition(List<string> words, string letter, int position)
        {
            int count = 0;
            foreach (var word in words)
            {
                if (word.Substring(position, 1) == letter)
                {
                    count++;
                }
            }
            return count;
        }

        private static List<string> GetWordsWithLetterInPosition(List<string> words, string letter, int position)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                if (word.Substring(position, 1) == letter)
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        private static List<string> GetWordsWithNoLetterInPosition(List<string> words, string letter, int position)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                if (word.Substring(position, 1) != letter)
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }
        private static List<string> GetWordsWithNoLetter(List<string> words, string letter, List<LetterPosition> correctLetters)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                string changedWord = GetWordWithoutCorrectLetters(word, correctLetters);
                if (!changedWord.Contains(letter))
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        public static string ChooseRandomWord(List<string> words, Random randomizer)
        {
            int index = randomizer.Next(0, words.Count);
            return words[index];
        }

        private static List<string> GetListOfWordsWithLetter(string letter, List<string> wordList)
        {
            List<string> wordsWithLetter = new List<string>();
            foreach (var word in wordList)
            {
                if (word.Contains(letter))
                {
                    wordsWithLetter.Add(word);
                }
            }
            return wordsWithLetter;
        }

        private static List<string> GetListOfWordsWithLetters(string[] letters, List<string> wordList)
        {
            List<string> wordsWithLetter = new List<string>();
            foreach (var word in wordList)
            {
                bool doesContainLetters = true;
                for (int i = 0; i < letters.Length; i++)
                {
                    if (!word.Contains(letters[i]))
                    {
                        doesContainLetters = false;
                        break;
                    }
                }
                if (doesContainLetters)
                {
                    wordsWithLetter.Add(word);
                }
            }
            return wordsWithLetter;
        }

        private static List<string> GetWordsWithLetterNotInPositions(List<string> words, string letter, List<LetterPosition> correctLetters)
        {
            List<string> newWordList = new List<string>();
            foreach (var word in words)
            {
                string changedWord = GetWordWithoutCorrectLetters(word, correctLetters);
                if (!changedWord.Contains(letter))
                {
                    newWordList.Add(word);
                }
            }
            return newWordList;
        }

        // TODO clean up this method - refactor
        public static WordGuess GetGuessResults(string guessWord, string correctWord)
        {
            WordGuess guess = new WordGuess(guessWord);
            List<LetterCount> containedLetters = new List<LetterCount>();

            for (int i = 0; i < guessWord.Length; i++)
            {
                string guessLetter = guessWord.Substring(i, 1);
                string correctLetter = correctWord.Substring(i, 1);
                LetterStatus result;


                if (guessLetter == correctLetter)
                {
                    result = LetterStatus.Correct;

                    // TODO move this to own method
                    bool isInContainedList = false;
                    int timesInContainedList = 1;
                    LetterCount containedLetterCount = new LetterCount()
                    {
                        Letter = guessLetter,
                        Count = timesInContainedList
                    };
                    int containedLetterIndex = 0;
                    containedLetters.ForEach(l => { });
                    for (int n = 0; n < containedLetters.Count; n++)
                    {
                        LetterCount l = containedLetters[n];
                        if (l.Letter == guessLetter)
                        {
                            l.Count++;
                            timesInContainedList = l.Count;
                            containedLetterCount = l;
                            isInContainedList = true;
                            containedLetterIndex = n;
                        }
                    }
                    if (!isInContainedList)
                    {
                        containedLetters.Add(containedLetterCount);
                        containedLetterIndex = containedLetters.Count - 1;
                    }

                    // if there are too many letter instances in contained list, change one of the previous "Wrong Position" guess letters to being incorrect
                    int timesInCorrectWord = CountInstancesInWord(guessLetter, correctWord);
                    if (timesInContainedList > timesInCorrectWord)
                    {
                        do
                        {
                            foreach (var guessResult in guess.Result)
                            {
                                if (guessResult != null && guessResult.Letter == guessLetter && guessResult.Status == LetterStatus.WrongPosition)
                                {
                                    guessResult.Status = LetterStatus.Incorrect;
                                    timesInContainedList--;
                                    containedLetters[containedLetterIndex].Count = timesInContainedList;
                                    break;
                                }
                            }
                        } while (timesInContainedList > timesInCorrectWord);
                    }
                    ////////////////////////////
                }
                else if (correctWord.Contains(guessLetter))
                {
                    int timesInCorrectWord = CountInstancesInWord(guessLetter, correctWord);
                    int timesInContainedList = 1;

                    // TODO move this to own method
                    bool isInContainedList = false;
                    LetterCount containedLetterCount = new LetterCount()
                    {
                        Letter = guessLetter,
                        Count = timesInContainedList
                    };
                    ;
                    containedLetters.ForEach(l => { if (l.Letter == guessLetter) { l.Count++; containedLetterCount = l; timesInContainedList = l.Count; isInContainedList = true; } });
                    if (!isInContainedList)
                    {
                        containedLetters.Add(containedLetterCount);
                    }
                    ////////////////////////////

                    if (timesInContainedList <= timesInCorrectWord)
                    {
                        result = LetterStatus.WrongPosition;
                    }
                    else
                    {
                        result = LetterStatus.Incorrect;
                        containedLetterCount.Count--;
                    }
                }
                else
                {
                    result = LetterStatus.Incorrect;
                }

                guess.Result[i] = new LetterPosition()
                {
                    Letter = guessLetter,
                    Position = i,
                    Status = result
                };
            }

            // determine if guess was correct or not
            bool isCorrect = true;
            foreach (var result in guess.Result)
            {
                if (result.Status != LetterStatus.Correct)
                {
                    isCorrect = false;
                    break;
                }
            }

            guess.IsCorrect = isCorrect;
            return guess;
        }

        public static string GetGuessWordByLetterDistribution(List<string> words,
            WordGuess guess, List<LetterWrongPositions> wrongPosLetters)
        {
            if (words.Count == 0)
            {
                return "error";
            }

            List<WordProbability> probabilities = GetWordScoresByLetterDistribution(words, guess,
                wrongPosLetters);
            return probabilities[0].Word;
        }

        public static string GetGuessWordByCompleteLetterDistribution(List<string> words, List<string> allWords,
            List<LetterWrongPositions> wrongPosLetters)
        {
            if (words.Count == 0)
            {
                return "error";
            }

            List<WordProbability> probabilities = GetCompleteWordScoresByLetterDistribution(words, allWords, wrongPosLetters);
            return probabilities[0].Word;
        }

        public static List<WordProbability> GetWordScoresByLetterDistribution(List<string> words,
            WordGuess guess, List<LetterWrongPositions> wrongPosLetters)
        {
            List<int> unknownPositions = new List<int>();
            List<int> wrongPositions = new List<int>();
            foreach (var result in guess.Result)
            {
                if (result.Status != LetterStatus.Correct)
                {
                    unknownPositions.Add(result.Position);
                }
                else if (result.Status != LetterStatus.WrongPosition)
                {
                    wrongPositions.Add(result.Position);
                }
            }
            List<PositionLetterFrequencies> unknownFrequencies = GetLetterFrequenciesInUnknownPositions(words, unknownPositions, allLetters);
            List<PositionLetterFrequencies> wrongPosFrequencies = GetLetterFrequenciesInWrongPositions(words, wrongPosLetters);
            List<WordProbability> wordScores = new List<WordProbability>();
            foreach (var word in words)
            {
                wordScores.Add(GetWordScoreByLetterDistribution(word, unknownFrequencies, wrongPosFrequencies));
            }

            return wordScores.OrderBy(w => w.Score).Reverse().ToList();
        }

        public static List<LetterWrongPositions> AddToWrongPositionLetters(List<LetterWrongPositions> letterPosList, WordGuess newGuess,
            List<LetterPosition> newCorrectPos)
        {
            // find which letters in the new guess are wrong positions
            LetterPosition[] newResults = newGuess.Result;

            // remove any new correct positions from the letterPosList if it matches letter and possible position
            List<LetterWrongPositions> toRemove = new List<LetterWrongPositions>();
            foreach (var newCorrect in newCorrectPos)
            {
                foreach (var lPos in letterPosList)
                {
                    if (newCorrect.Letter == lPos.Letter)
                    {
                        bool alreadyInCopy = false;
                        if (toRemove.Contains(lPos))
                        {
                            alreadyInCopy = true;
                        }
                        if (!alreadyInCopy)
                        {
                            toRemove.Add(lPos);
                        }
                    }
                }
            }

            // now remove any letter poses contained in toRemove
            List<LetterWrongPositions> letterPosCopy = new List<LetterWrongPositions>();
            foreach (var le in letterPosList)
            {
                bool isContained = false;
                foreach (var r in toRemove)
                {
                    bool isCopy = true;
                    if (le.Letter != r.Letter)
                    {
                        isCopy = false;
                    }
                    if (isCopy)
                    {
                        if (le.PossiblePositions.Count != r.PossiblePositions.Count)
                        {
                            isCopy = false;
                        }
                        if (isCopy)
                        {
                            for (int t = 0; t < r.PossiblePositions.Count; t++)
                            {
                                if (r.PossiblePositions[t] != le.PossiblePositions[t])
                                {
                                    isCopy = false;
                                    break;
                                }
                            }
                            if (isCopy)
                            {
                                if (le.WrongPositions.Count != r.WrongPositions.Count)
                                {
                                    isCopy = false;
                                }
                                if (isCopy)
                                {
                                    for (int t = 0; t < r.WrongPositions.Count; t++)
                                    {
                                        if (r.WrongPositions[t] != le.WrongPositions[t])
                                        {
                                            isCopy = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (isCopy)
                    {
                        isContained = true;
                    }
                }

                if (!isContained)
                {
                    letterPosCopy.Add(le);
                }

            }
            letterPosList = letterPosCopy;

            List<int> correctPositions = new List<int>();
            // add correct positions to list so those can be added to wrong positions for letters
            for (int i = 0; i < newResults.Length; i++)
            {
                if (newResults[i].Status == LetterStatus.Correct)
                {
                    correctPositions.Add(newResults[i].Position);
                }
            }

            // now do wrong letter positions
            for (int i = 0; i < newResults.Length; i++)
            {
                if (newResults[i].Status == LetterStatus.WrongPosition)
                {
                    // if it's the wrong position, see if that letter is in the list already
                    bool isInList = false;
                    int wrongPosCount = letterPosList.Where(l => l.Letter == newResults[i].Letter).Count();
                    foreach (var item in letterPosList)
                    {
                        // count how many times it's in both lists
                        int resultCount = newResults.Where(r => r.Letter == item.Letter && r.Status == LetterStatus.WrongPosition)
                            .Count();
                        if (item.Letter == newResults[i].Letter)
                        {

                            if (wrongPosCount == resultCount)
                            {
                                isInList = true;
                                // if so, now see if that position is there still
                                if (!item.WrongPositions.Contains(newResults[i].Position))
                                {
                                    item.WrongPositions.Add(newResults[i].Position);
                                }
                                break;
                            }
                        }
                    }

                    if (!isInList) // if that letter isn't counted yet, add it
                    {
                        // if it counts as "not in list" but that letter exists in the list, that means
                        // there are duplicates and add that spot to the existing ones
                        if (wrongPosCount > 0)
                        {
                            foreach (var l in letterPosList)
                            {
                                if (l.Letter == newResults[i].Letter &&
                                    !l.WrongPositions.Contains(newResults[i].Position))
                                {
                                    l.WrongPositions.Add(wrongPosCount);
                                }
                            }
                        }
                        letterPosList.Add(new LetterWrongPositions(newResults[i].Letter, newResults[i].Position));

                        // if there are duplicate letters, set them all to the one with the most wrong positions listed
                        if (wrongPosCount > 0)
                        {
                            List<LetterWrongPositions> duplicates = letterPosList.Where(l => l.Letter == newResults[i].Letter).ToList();
                            List<int> longest = new List<int>();
                            foreach (var d in duplicates)
                            {
                                if (d.WrongPositions.Count > longest.Count)
                                {
                                    longest = d.WrongPositions;
                                }
                            }

                            foreach (var l in letterPosList)
                            {
                                if (l.Letter == newResults[i].Letter)
                                {
                                    l.WrongPositions = longest;
                                }
                            }
                        }
                    }

                }
            }

            // now add correct positions to wrong positions
            foreach (var c in correctPositions)
            {
                // now add correct letter positions
                foreach (var item in letterPosList)
                {
                    if (!item.WrongPositions.Contains(c))
                    {
                        item.WrongPositions.Add(c);
                    }
                }
            }

            // now calculate the possible positions for each item in wrong position letters
            // now add correct letter positions
            foreach (var item in letterPosList)
            {
                item.PossiblePositions.Clear();
                int[] allPos = new int[] { 0, 1, 2, 3, 4 };
                for (int i = 0; i < allPos.Length; i++)
                {
                    if (!item.WrongPositions.Contains(allPos[i]))
                    {
                        item.PossiblePositions.Add(allPos[i]);
                    }
                }
            }

            // double check that no positions are in the list that have no possibilities - because something was missed
            List<LetterWrongPositions> letterPosCopy2 = new List<LetterWrongPositions>();
            foreach (var le in letterPosList)
            {
                if (le.PossiblePositions.Count != 0)
                {
                    letterPosCopy2.Add(le);
                }
            }
            letterPosList = letterPosCopy2;


            return letterPosList;
        }

        public static string GetInitialGuessWord(List<string> words, List<string> allWords,
            string[] commonLetters)
        {
            if (words.Count == 0)
            {
                return "error";
            }

            List<WordProbability> probabilities = GetInitialWordScores(words, allWords, commonLetters);
            return probabilities[0].Word;
        }

        public static List<WordProbability> GetInitialWordScores(List<string> words,
            List<string> allWords, string[] commonLetters)
        {
            List<int> unknownPositions = new List<int>() { 0, 1, 2, 3, 4 };
            List<PositionLetterFrequencies> frequencies = GetLetterFrequenciesInUnknownPositions(allWords, unknownPositions, commonLetters);

            List<WordProbability> wordScores = new List<WordProbability>();
            foreach (var word in words)
            {
                wordScores.Add(GetWordScoreByLetterDistribution(word, frequencies));
            }

            return wordScores.OrderBy(w => w.Score).Reverse().ToList();
        }

        public static List<WordProbability> GetCompleteWordScoresByLetterDistribution(List<string> words, List<string> allWords,
            List<LetterWrongPositions> wrongPosLetters)
        {
            List<int> unknownPositions = new List<int>() { 0, 1, 2, 3, 4 };
            List<PositionLetterFrequencies> frequencies = GetLetterFrequenciesInUnknownPositions(allWords, unknownPositions, allLetters);
            List<PositionLetterFrequencies> wrongPosFrequencies = GetLetterFrequenciesInWrongPositions(words, wrongPosLetters);
            List<WordProbability> wordScores = new List<WordProbability>();
            foreach (var word in words)
            {
                wordScores.Add(GetWordScoreByLetterDistribution(word, frequencies, wrongPosFrequencies));
            }

            return wordScores.OrderBy(w => w.Score).Reverse().ToList();
        }

        public static WordProbability GetWordScoreByLetterDistribution(string word,
            List<PositionLetterFrequencies> positionLetterFrequencies)
        {
            double score = 0;
            foreach (var frequency in positionLetterFrequencies)
            {
                string posLetter = word.Substring(frequency.Position, 1);
                foreach (var letter in frequency.LetterCounts)
                {
                    if (letter.Letter == posLetter)
                    {
                        score += letter.Count;
                        break;
                    }
                }
            }

            return new WordProbability()
            {
                Word = word,
                Score = score
            };
        }

        public static WordProbability GetWordScoreByLetterDistribution(string word,
            List<PositionLetterFrequencies> positionLetterFrequencies,
            List<PositionLetterFrequencies> wrongLetterFrequencies)
        {
            double score = 0;
            foreach (var frequency in positionLetterFrequencies)
            {
                string posLetter = word.Substring(frequency.Position, 1);
                foreach (var letter in frequency.LetterCounts)
                {
                    if (letter.Letter == posLetter)
                    {
                        score += letter.Count;
                        break;
                    }
                }
            }

            foreach (var frequency in wrongLetterFrequencies)
            {
                string posLetter = word.Substring(frequency.Position, 1);
                foreach (var letter in frequency.LetterCounts)
                {
                    if (letter.Letter == posLetter)
                    {
                        score += (letter.Count * 0.5);
                        break;
                    }
                }
            }

            return new WordProbability()
            {
                Word = word,
                Score = score
            };
        }

        public static List<PositionLetterFrequencies> GetLetterFrequenciesInUnknownPositions(List<string> words,
            List<int> unknownPositions, string[] letters)
        {
            List<PositionLetterFrequencies> positionFrequencies = new List<PositionLetterFrequencies>();
            foreach (var position in unknownPositions)
            {
                positionFrequencies.Add(new PositionLetterFrequencies()
                {
                    Position = position,
                    LetterCounts = GetLetterFrequenciesInPosition(words, position, letters)
                });
            }
            return positionFrequencies;
        }

        public static List<PositionLetterFrequencies> GetLetterFrequenciesInWrongPositions(List<string> words,
            List<LetterWrongPositions> wrongPosLetters)
        {
            List<PositionLetterFrequencies> positionFrequencies = new List<PositionLetterFrequencies>();
            int[] allPos = new int[] { 0, 1, 2, 3, 4 };
            for (int i = 0; i < allPos.Length; i++)
            {
                int pos = allPos[i];
                int letterCount = 0;
                List<string> lettersList = new List<string>();
                foreach (var letter in wrongPosLetters)
                {
                    if (letter.PossiblePositions.Contains(pos))
                    {
                        letterCount++;
                        lettersList.Add(letter.Letter);
                    }
                }
                string[] lettersArray = lettersList.ToArray();
                List<LetterCount> letterCounts = GetLetterFrequenciesInPosition(words, pos, lettersArray);
                if (letterCounts.Count != 0)
                {
                    positionFrequencies.Add(new PositionLetterFrequencies()
                    {
                        Position = pos,
                        LetterCounts = letterCounts
                    });
                }
            }
            return positionFrequencies;
        }

        public static List<PositionLetterFrequencies> GetLetterPositionFrequenciesByCommon(List<string> words,
            string[] commonLetters)
        {
            List<int> unknownPositions = new List<int>() { 0, 1, 2, 3, 4 };
            List<PositionLetterFrequencies> positionFrequencies = new List<PositionLetterFrequencies>();
            foreach (var position in unknownPositions)
            {
                positionFrequencies.Add(new PositionLetterFrequencies()
                {
                    Position = position,
                    LetterCounts = GetLetterFrequenciesInPosition(words, position, commonLetters)
                });
            }
            return positionFrequencies;
        }

        public static List<LetterCount> GetLetterFrequenciesInPosition(List<string> words,
            int position, string[] letters)
        {
            List<LetterCount> frequencies = new List<LetterCount>();
            for (int i = 0; i < letters.Length; i++)
            {
                string letter = letters[i];
                int count = 0;

                foreach (var word in words)
                {
                    if (word.Substring(position, 1) == letter)
                    {
                        count++;
                    }
                }

                LetterCount letterCount = new LetterCount()
                {
                    Letter = letter,
                    Count = count
                };
                frequencies.Add(letterCount);
            }

            return frequencies.OrderBy(l => l.Count).Reverse().ToList();
        }

        public static List<string> GetAllWords(WordListType listType = WordListType.Scrabble)
        {
            //string dictionaryPath = $".\\files\\word-list.txt";
            string dictionaryPath;
            switch (listType)
            {
                default:
                case WordListType.Scrabble:
                    dictionaryPath = $".\\files\\word-list-scrabble.txt";
                    break;
                case WordListType.Wordle:
                    dictionaryPath = $".\\files\\wordle-official-all.txt";
                    break;
                case WordListType.WordleCommon:
                    dictionaryPath = $".\\files\\wordle-official-common.txt";
                    break;
                case WordListType.Suggested:
                    dictionaryPath = $".\\files\\word-list-suggested.txt";
                    break;
                case WordListType.Full:
                    dictionaryPath = $".\\files\\word-list-full.txt";
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

        public static List<string> GetAllUserGuessWords()
        {
            //string dictionaryPath = $".\\files\\word-list.txt";
            string dictionaryPath = $".\\files\\wordle-official-common.txt";

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

        public static List<string> GetUserWordsNotIncluded()
        {
            List<string> userWords = GetAllUserGuessWords();
            List<string> allWords = GetAllWords();

            List<string> wordsNotIncluded = new List<string>();

            userWords.ForEach(uw =>
            {
                bool isIncluded = false;
                foreach (var word in allWords)
                {
                    if (uw == word)
                    {
                        isIncluded = true;
                        break;
                    }
                }

                if (!isIncluded)
                {
                    wordsNotIncluded.Add(uw);
                }
            });

            return wordsNotIncluded;
        }


        private static int CountInstancesInWord(string letter, string word)
        {
            int count = 0;
            for (int i = 0; i < word.Length; i++)
            {
                if (word.Substring(i, 1) == letter)
                {
                    count++;
                }
            }
            return count;
        }

        public static string GetMostCommonLetter(List<string> words)
        {
            List<LetterCount> countsByHighest = GetLetterCountsInOrder(words);
            LetterCount highestCount = countsByHighest[0];
            return highestCount.Letter;
        }

        public static string[] GetMostCommonLetters(List<string> words, int howManyLetters)
        {
            string[] letters = new string[howManyLetters];
            List<LetterCount> countsByHighest = GetLetterCountsInOrder(words);

            for (int i = 0; i < howManyLetters; i++)
            {
                letters[i] = countsByHighest[i].Letter;
            }

            return letters;
        }

        public static string GetMostCommonLetterInPosition(List<string> words, int position)
        {
            List<LetterCount> countsByHighest = GetLetterCountsInOrderInPosition(words, position);
            LetterCount highestCount = countsByHighest[0];
            return highestCount.Letter;
        }

        // count how many times each letter appears in a dictionary of 5 letter words - sort them by most to least
        public static List<LetterCount> GetLetterCountsInOrder(List<string> words)
        {
            List<LetterCount> letterCounts = new List<LetterCount>();

            for (int i = 0; i < allLetters.Length; i++)
            {
                string letter = allLetters[i];
                int count = 0;

                foreach (var word in words)
                {
                    if (word.Contains(letter))
                    {
                        for (int n = 0; n < word.Length; n++)
                        {
                            if (word.Substring(n, 1) == letter)
                            {
                                count++;
                            }
                        }
                    }
                }

                letterCounts.Add(new LetterCount()
                {
                    Letter = letter,
                    Count = count
                });

            }

            return letterCounts.OrderBy(w => w.Count).Reverse().ToList();
        }

        // count how many times each letter appears in a dictionary of 5 letter words - sort them by most to least
        public static List<LetterCount> GetLetterCountsInOrderNotInPositions(List<string> words, List<LetterPosition> notPositions)
        {
            List<LetterCount> letterCounts = new List<LetterCount>();

            for (int i = 0; i < allLetters.Length; i++)
            {
                string letter = allLetters[i];
                int count = 0;

                foreach (var word in words)
                {
                    string changedWord = GetWordWithoutCorrectLetters(word, notPositions);
                    if (changedWord.Contains(letter))
                    {
                        for (int n = 0; n < changedWord.Length; n++)
                        {
                            if (changedWord.Substring(n, 1) == letter)
                            {
                                count++;
                            }
                        }
                    }
                }

                letterCounts.Add(new LetterCount()
                {
                    Letter = letter,
                    Count = count
                });

            }

            return letterCounts.OrderBy(w => w.Count).Reverse().ToList();
        }

        // count how many times each letter appears in a dictionary of 5 letter words - sort them by most to least
        public static List<LetterCount> GetLetterCountsInOrderInPosition(List<string> words, int position)
        {
            List<LetterCount> letterCounts = new List<LetterCount>();

            for (int i = 0; i < allLetters.Length; i++)
            {
                string letter = allLetters[i];
                int count = 0;

                foreach (var word in words)
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

            return letterCounts.OrderBy(w => w.Count).Reverse().ToList();
        }

        public static bool DoesWordExist(string word)
        {
            List<string> words = GetAllWords(WordListType.Full);
            bool doesExist = false;

            foreach (var w in words)
            {
                if (w.Trim() == word)
                {
                    doesExist = true;
                    break;
                }
            }
            return doesExist;
        }

        /// <summary>
        /// This provides a list of all failing words in a range of words.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="howMany"></param>
        /// <returns></returns>
        public static List<string> GetFailingWords(int startIndex, int howMany)
        {
            List<string> allWords = GetAllWords(WordListType.Scrabble);
            int endIndex = startIndex + howMany;
            if (endIndex >= allWords.Count)
            {
                endIndex = allWords.Count - 1;
            }

            List<string> failedWord = new List<string>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                string word = allWords[i];
                WordleGuesses result = GuessWordleSolution(word);
                if (!result.DidWin)
                {
                    failedWord.Add(result.CorrectAnswer);
                }
            }
            return failedWord;
        }

        public static List<string> GetWordsNotInSecondList(WordListType list1Type = WordListType.WordleCommon, WordListType list2Type = WordListType.Scrabble)
        {
            List<string> list1 = GetAllWords(list1Type);
            List<string> list2 = GetAllWords(list2Type);
            List<string> notIncluded = new List<string>();

            foreach (var word in list1)
            {
                if (!list2.Contains(word))
                {
                    notIncluded.Add(word);
                }
            }
            return notIncluded;
        }

    }
}
