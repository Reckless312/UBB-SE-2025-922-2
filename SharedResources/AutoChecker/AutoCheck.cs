// <copyright file="AutoCheck.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using IRepository;
    using Microsoft.Data.SqlClient;

    public class AutoCheck : IAutoCheck
    {
        private readonly IOffensiveWordsRepository repository;
        private readonly HashSet<string> offensiveWords;

        private static readonly char[] WordDelimiters = new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' };

        public AutoCheck(IOffensiveWordsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.offensiveWords = this.repository.LoadOffensiveWords();
        }

        public bool AutoCheckReview(string reviewText)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
            {
                return false;
            }

            string[] words = reviewText.Split(WordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(word => offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase));
        }

        public void AddOffensiveWord(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }

            if (offensiveWords.Contains(newWord, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            repository.AddWord(newWord);
            offensiveWords.Add(newWord);
        }

        public void DeleteOffensiveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            if (!offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            repository.DeleteWord(word);
            offensiveWords.Remove(word);
        }

        public HashSet<string> GetOffensiveWordsList()
        {
            return new HashSet<string>(offensiveWords, StringComparer.OrdinalIgnoreCase);
        }
    }
}