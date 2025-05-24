namespace IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IOffensiveWordsRepository
    {
        Task<HashSet<string>> LoadOffensiveWords();

        Task AddWord(string word);

        Task DeleteWord(string word);
    }
}
