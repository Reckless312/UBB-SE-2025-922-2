namespace CombinedProject.AutoChecker
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
