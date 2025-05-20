namespace DataAccess.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface ICheckersService
    {
        public Task<List<string>> RunAutoCheck(List<Review> reviews);

        public HashSet<string> GetOffensiveWordsList();

        public Task AddOffensiveWordAsync(string newWord);

        public Task DeleteOffensiveWordAsync(string word);

        public Task RunAICheckForOneReviewAsync(Review review);
    }
}
