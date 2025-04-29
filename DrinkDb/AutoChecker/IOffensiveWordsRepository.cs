// <copyright file="IOffensiveWordsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IOffensiveWordsRepository
    {
        HashSet<string> LoadOffensiveWords();

        void AddWord(string word);

        void DeleteWord(string word);
    }
}
