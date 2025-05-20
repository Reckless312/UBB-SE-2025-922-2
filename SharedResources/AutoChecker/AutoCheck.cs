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
    using System.Threading.Tasks;

    public class AutoCheck : IAutoCheck
    {
        private readonly IOffensiveWordsRepository repository;
        private HashSet<string> offensiveWords;

        private static readonly char[] WordDelimiters = new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' };

        public AutoCheck(IOffensiveWordsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            InitializeOffensiveWords().GetAwaiter().GetResult();
        }

        private async Task InitializeOffensiveWords()
        {
            this.offensiveWords = await this.repository.LoadOffensiveWords();
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

        public async Task AddOffensiveWordAsync(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }

            if (offensiveWords.Contains(newWord, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            await repository.AddWord(newWord);
            offensiveWords.Add(newWord);
        }

        public async Task DeleteOffensiveWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            if (!offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            await repository.DeleteWord(word);
            offensiveWords.Remove(word);
        }

        public HashSet<string> GetOffensiveWordsList()
        {
            return new HashSet<string>(offensiveWords, StringComparer.OrdinalIgnoreCase);
        }
    }
}