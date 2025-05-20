using DataAccess.Model.AdminDashboard;
using DataAccess.Model.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/offensiveWords")]
    public class OffensiveWordsController: ControllerBase
    {
        private readonly ICheckersService checkersService;

        public OffensiveWordsController(ICheckersService checkersService)
        {
            this.checkersService = checkersService ?? throw new ArgumentNullException(nameof(checkersService));
        }

        [HttpGet]
        public List<OffensiveWord> GetAllWords()
        {
            HashSet<string> words = this.checkersService.GetOffensiveWordsList();
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
            this.checkersService.AddOffensiveWord(word.Word);
        }

        [HttpDelete("delete/{word}")]
        public void DeleteWord(string word)
        {
            this.checkersService.DeleteOffensiveWord(word);
        }

        [HttpPost("check")]
        public List<string> CheckReviews([FromBody] List<Review> reviews)
        {
            return this.checkersService.RunAutoCheck(reviews);
        }

        [HttpPost("checkOne")]
        public void CheckOneReview([FromBody] Review review)
        {
            this.checkersService.RunAICheckForOneReview(review);
        }
    }
}
