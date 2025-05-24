namespace DataAccess.Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using ServerAPI.Data;

    public class OffensiveWordsRepository : IOffensiveWordsRepository
    {
        private DatabaseContext databaseContext;

        public OffensiveWordsRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<HashSet<string>> LoadOffensiveWords()
        {
            return await Task.FromResult(this.databaseContext.OffensiveWords
                .Select(w => w.Word)
                .ToHashSet(StringComparer.OrdinalIgnoreCase));
        }

        public async Task AddWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            bool exists = this.databaseContext.OffensiveWords.Any(item => item.Word == word);

            if (!exists)
            {
                this.databaseContext.OffensiveWords.Add(new OffensiveWord { Word = word });
                await this.databaseContext.SaveChangesAsync();
            }
        }

        public async Task DeleteWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            OffensiveWord? item = this.databaseContext.OffensiveWords.FirstOrDefault(item => item.Word == word);

            if (item == null)
            {
                return;
            }

            this.databaseContext.OffensiveWords.Remove(item);
            await databaseContext.SaveChangesAsync();
        }
    }
}
