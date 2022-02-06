using System;
using System.Collections.Generic;
using System.IO;

namespace WordleSolverAPI.Logic.Helpers
{
    public static class FailAnalyzer
    {
        public static List<string> GetFailedWords()
        {
            //string dictionaryPath = $".\\files\\word-list.txt";
            string dictionaryPath = $".\\files\\failed-words.txt";

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
