// <copyright file="IOffensiveWordsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IOffensiveWordsRepository
    {
        Task<HashSet<string>> LoadOffensiveWords();

        Task AddWord(string word);

        Task DeleteWord(string word);


    }
}
