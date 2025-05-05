using DataAccess.Model.AdminDashboard;
using DataAccess.Model.AutoChecker;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using Repository.AdminDashboard;
using ServerAPI.Data;
using ServerAPI.Repository.AutoChecker;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("offensiveWords")]
    public class OffensiveWordsController: ControllerBase
    {
        private IOffensiveWordsRepository repository;

        public OffensiveWordsController(IOffensiveWordsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public List<OffensiveWord> GetAllWords()
        {
            HashSet<string> words = repository.LoadOffensiveWords();
            List<OffensiveWord> wordsList = new List<OffensiveWord>();
            foreach (string word in words)
            {
                wordsList.Add(new OffensiveWord { Word = word });
            }
            return wordsList;
        }
        [HttpPost("add")]
        public void AddOffensiveWord(OffensiveWord word)
        {
            repository.AddWord(word.Word);
        }
        [HttpDelete("delete/{word}")]
        public void DeleteWord(string word)
        {
            repository.DeleteWord(word);
        }
    }
}
