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
        public bool AutoCheckReview(string reviewText);

        public void AddOffensiveWord(string newWord);

        public void DeleteOffensiveWord(string word);

        public HashSet<string> GetOffensiveWordsList();
    }
}
