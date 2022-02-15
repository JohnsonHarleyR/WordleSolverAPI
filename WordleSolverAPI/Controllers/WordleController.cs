using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using WordleSolverAPI.Logic;
using WordleSolverAPI.Logic.Models;

namespace WordleSolverAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [EnableCors]
    public class WordleController : ControllerBase
    {

        private readonly ILogger<WordleController> _logger;

        public WordleController(ILogger<WordleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult GetSolverStatistics(int timesToRun, bool allowWordsEndingInS)
        {
            return new JsonResult(WordSorter.GetStatistics(timesToRun, allowWordsEndingInS));
        }

        [HttpGet]
        public ActionResult AnalyzeFailedWords()
        {
            return new JsonResult(FailAnalyzer.GetFailedWordAnalysis());
        }

        [HttpGet]
        public ActionResult GetWordleGuesses(string correctAnswer)
        {
            WordleGuesses result = WordSorter.GuessWordleSolution(correctAnswer);

            return new JsonResult(result);
        }

        [HttpGet]
        public ActionResult GetGuessResults(string guessWord, string correctWord)
        {
            WordGuess guessResults = WordSorter.GetGuessResults(guessWord, correctWord);

            return new JsonResult(guessResults);
        }

        [HttpGet]
        public ActionResult DoesWordExist(string word)
        {
            return new JsonResult(WordSorter.DoesWordExist(word));
        }

        [HttpGet]
        public ActionResult GetWordleGuessesWithRandomAnswer()
        {
            Random random = new Random();
            string correctAnswer = WordSorter.ChooseRandomWord(WordSorter.GetAllWords(), random);
            WordleGuesses result = WordSorter.GuessWordleSolution(correctAnswer);

            return new JsonResult(result);
        }

        [HttpGet]
        public ActionResult GetRandomGuessWord()
        {
            Random random = new Random();
            List<string> allWords = WordSorter.GetAllUserGuessWords();
            int index = random.Next(0, allWords.Count);
            return new JsonResult(allWords[index]);
        }

        [HttpGet]
        public ActionResult GetEmptyRound(string correctAnswer)
        {
            WordleGuesses emptyRound = new WordleGuesses();
            emptyRound.CorrectAnswer = correctAnswer;
            return new JsonResult(emptyRound);
        }


        [HttpGet]
        public ActionResult GetAllWords()
        {
            return new JsonResult(WordSorter.GetAllWords());
        }

        [HttpGet]
        public ActionResult GetFailedWords(int startIndex, int howMany)
        {
            return new JsonResult(WordSorter.GetFailingWords(startIndex, howMany));
        }

        [HttpGet]
        public ActionResult GetUserGuessWords()
        {
            return new JsonResult(WordSorter.GetAllUserGuessWords());
        }

        [HttpGet]
        public ActionResult GetUserGuessWordsNotIncluded()
        {
            return new JsonResult(WordSorter.GetUserWordsNotIncluded());
        }

        [HttpGet]
        public ActionResult AnalyzeFailedPositionFrequencies()
        {
            return new JsonResult(FailAnalyzer.AnalyzeFailedPositionLetters());
        }

        //[HttpGet]
        //public ActionResult GetWordsWithPattern(string pattern, bool checkFailing = false, bool includeSEnding = true)
        //{
        //    return new JsonResult(WordSorter.GetWordsWithPattern(pattern, checkFailing, includeSEnding));
        //}

        [HttpGet]
        public ActionResult GetWordsWithPatternAndAnalysis(string pattern, bool checkFailing = false, bool includeSEnding = true)
        {
            return new JsonResult(WordSorter.AnalyzePattern(pattern, checkFailing, includeSEnding));
        }


        //[HttpGet]
        //public ActionResult GetPercentOfWordsWithPattern(string pattern, bool checkFailing = false, bool includeSEnding = true)
        //{
        //    return new JsonResult(WordSorter.GetPercentOfWordsWithPattern(pattern, checkFailing, includeSEnding));
        //}

        [HttpGet]
        public ActionResult GetFailinggWordsThatMatchPatternFromFailedList(string pattern)
        {
            return new JsonResult(FailAnalyzer.GetFailinggWordsThatMatchPatternFromFailedList(pattern));
        }

        [HttpGet]
        public ActionResult GetPercentOfWordsWithPatternNowPassing(string pattern)
        {
            return new JsonResult(FailAnalyzer.GetPercentOfWordsWithPatternNowPassing(pattern));
        }

        [HttpGet]
        public ActionResult GetWordsWithPatternNowPassing(string pattern)
        {
            return new JsonResult(FailAnalyzer.GetWordsWithPatternNowPassing(pattern));
        }

        [HttpGet]
        public ActionResult GetWordsWithPatternNowFailing(string pattern)
        {
            return new JsonResult(FailAnalyzer.GetWordsWithPatternNowFailing(pattern));
        }

        [HttpGet]
        public ActionResult GetPercentOfWordsWithPatternAndDoubles(string pattern, bool checkFailing = false, bool includeSEnding = true)
        {
            List<string> wordsMatchingPattern = WordSorter.GetWordsWithPattern(pattern, checkFailing, includeSEnding);
            List<string> wordsWithDoubles = FailAnalyzer.GetWordsWithDoubleLetters(wordsMatchingPattern);
            StringPercentContainer container = new StringPercentContainer()
            {
                String = pattern,
                Percent = FailAnalyzer.GetPercent(wordsWithDoubles.Count, wordsMatchingPattern.Count),
                ExampleList = wordsWithDoubles
            };
            return new JsonResult(container);
        }

        [HttpGet]
        public ActionResult CountWordsWithPattern(string pattern, bool checkFailing = false)
        {
            return new JsonResult(WordSorter.CountWordsWithPattern(pattern, checkFailing));
        }

        [HttpGet]
        public ActionResult GetPassFailRatesWithPattern(string pattern, bool checkFailing = false)
        {
            return new JsonResult(WordSorter.GetPassFailRatesForWordsWithPattern(pattern, checkFailing));
        }
    }
}
