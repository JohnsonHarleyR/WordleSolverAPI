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
        public ActionResult GetSolverStatistics(int timesToRun)
        {
            return new JsonResult(WordSorter.GetStatistics(timesToRun));
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
            return new JsonResult(FailAnalyzer.GetFailingWords(startIndex, howMany));
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

    }
}
