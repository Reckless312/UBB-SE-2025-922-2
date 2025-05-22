// <copyright file="IAutoCheck.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public interface IAutoCheck
    {
        public Task<bool> AutoCheckReview(string reviewText);

        public Task AddOffensiveWordAsync(string newWord);

        public Task DeleteOffensiveWordAsync(string word);

        public Task<HashSet<string>> GetOffensiveWordsList();
    }
}
