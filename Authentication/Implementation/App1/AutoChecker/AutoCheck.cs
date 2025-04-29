// <copyright file="AutoCheck.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Provides functionality for automatically checking reviews for offensive content
    /// using a predefined list of offensive words stored in a database.
    /// </summary>
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
            return words.Any(word => this.offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase));
        }

        public void AddOffensiveWord(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }

            if (this.offensiveWords.Contains(newWord, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            this.repository.AddWord(newWord);
            this.offensiveWords.Add(newWord);
        }

        public void DeleteOffensiveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            if (!this.offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            this.repository.DeleteWord(word);
            this.offensiveWords.Remove(word);
        }

        public HashSet<string> GetOffensiveWordsList()
        {
            return new HashSet<string>(this.offensiveWords, StringComparer.OrdinalIgnoreCase);
        }
    }
}