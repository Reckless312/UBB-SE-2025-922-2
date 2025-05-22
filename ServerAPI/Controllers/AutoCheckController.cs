using DataAccess.AutoChecker;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace ServerAPI.Controllers
{

    [ApiController]
    [Route("api/autocheck")]
    public class AutoCheckController
    {
        private IOffensiveWordsRepository repository;
        private IAutoCheck autoChecker;

        public AutoCheckController(IOffensiveWordsRepository repository, IAutoCheck autoChecker)
        {
            this.repository = repository;
            this.autoChecker = autoChecker;
        }

        [HttpPost("add")]
        public async Task AddWord([FromQuery] string newWord)
        {
            await repository.AddWord(newWord);
        }

        [HttpPost("review")]
        public async Task<bool> ReviewText([FromBody] string reviewText)
        {
            return await autoChecker.AutoCheckReview(reviewText);
        }

        [HttpDelete("delete")]
        public async Task DeleteText([FromQuery] string word)
        {
            await repository.DeleteWord(word);
        }

        [HttpGet("words")]
        public async Task<HashSet<string>> GetAll()
        {
            return await repository.LoadOffensiveWords();
        }
    }
}
