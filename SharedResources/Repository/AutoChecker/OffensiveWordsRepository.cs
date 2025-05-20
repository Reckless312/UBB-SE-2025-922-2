// <copyright file="OffensiveWordsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ServerAPI.Repository.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AutoChecker;
    using global::Repository.AdminDashboard;
    using IRepository;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using ServerAPI.Data;

    public class OffensiveWordsRepository : IOffensiveWordsRepository
    {
        private readonly DatabaseContext databaseContext;

        public OffensiveWordsRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<HashSet<string>> LoadOffensiveWords()
        {
            return await Task.FromResult(databaseContext.OffensiveWords
                .Select(w => w.Word)
                .ToHashSet(StringComparer.OrdinalIgnoreCase));
        }

        public async Task AddWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            bool exists = databaseContext.OffensiveWords.Any(w => w.Word == word);
            if (!exists)
            {
                databaseContext.OffensiveWords.Add(new OffensiveWord { Word = word });
                await databaseContext.SaveChangesAsync();
            }
        }

        public async Task DeleteWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            var entity = databaseContext.OffensiveWords.FirstOrDefault(w => w.Word == word);
            if (entity != null)
            {
                databaseContext.OffensiveWords.Remove(entity);
                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
