namespace CombinedProject.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CombinedProject.Model;

    public interface ICheckersService
    {
        public List<string> RunAutoCheck(List<Review> reviews);

        public HashSet<string> GetOffensiveWordsList();

        public void AddOffensiveWord(string newWord);

        public void DeleteOffensiveWord(string word);

        public void RunAICheckForOneReview(Review review);
    }
}
