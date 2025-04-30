// <copyright file="ICheckersService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedResources.Model.AdminDashboard;

    public interface ICheckersService
    {
        public List<string> RunAutoCheck(List<Review> reviews);

        public HashSet<string> GetOffensiveWordsList();

        public void AddOffensiveWord(string newWord);

        public void DeleteOffensiveWord(string word);

        public void RunAICheckForOneReview(Review review);
    }
}
