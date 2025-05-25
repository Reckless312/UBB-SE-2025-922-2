using IRepository;

namespace DataAccess.AutoChecker
{
    public class AutoCheck : IAutoCheck
    {
        private IOffensiveWordsRepository repository;
        private HashSet<string> offensiveWords;

        private static readonly char[] WordDelimiters = new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' };

        public AutoCheck(IOffensiveWordsRepository repository)
        {
            this.repository = repository;
            this.offensiveWords = new HashSet<string>();
            InitializeOffensiveWords().GetAwaiter().GetResult();
        }

        private async Task InitializeOffensiveWords()
        {
            this.offensiveWords = await this.repository.LoadOffensiveWords();
        }

        public Task<bool> AutoCheckReview(string reviewText)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
            {
                return Task.FromResult(false);
            }

            string[] words = reviewText.Split(AutoCheck.WordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return Task.FromResult(words.Any(word => this.offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase)));
        }

        public async Task AddOffensiveWordAsync(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }

            if (this.offensiveWords.Contains(newWord, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            await this.repository.AddWord(newWord);
            this.offensiveWords.Add(newWord);
        }

        public async Task DeleteOffensiveWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            if (!this.offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            await this.repository.DeleteWord(word);
            this.offensiveWords.Remove(word);
        }

        public Task<HashSet<string>> GetOffensiveWordsList()
        {
            return Task.FromResult(new HashSet<string>(this.offensiveWords, StringComparer.OrdinalIgnoreCase));
        }
    }
}