using DataAccess.Model.AdminDashboard;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using Repository.AdminDashboard;
using ServerAPI.Data;
using ServerAPI.Repository.AutoChecker;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("offenssiveWords")]
    public class OffensiveWordsController: ControllerBase
    {
        private IOffensiveWordsRepository repository;

        public OffensiveWordsController(IOffensiveWordsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("")]
        public IActionResult GetAllWords()
        {
            var words = repository.LoadOffensiveWords();
            return Ok(words);
        }
        [HttpPost("add")]
        public void AddOffensiveWord(string word)
        {
            repository.AddWord(word);
        }
        [HttpDelete("delete")]
        public void DeleteWord(string word)
        {
            repository.DeleteWord(word);
        }
    }
}
