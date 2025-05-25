namespace DataAccess.AutoChecker
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAutoCheck
    {
        public Task<bool> AutoCheckReview(string reviewText);

        public Task AddOffensiveWordAsync(string newWord);

        public Task DeleteOffensiveWordAsync(string word);

        public Task<HashSet<string>> GetOffensiveWordsList();
    }
}
