using DataAccess.Model.AdminDashboard;
using DataAccess.Model.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IActionResult> AddOffensiveWord(OffensiveWord word)
        {
            try
            {
                await checkersService.AddOffensiveWordAsync(word.Word);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return null;
        }

        [HttpDelete("delete/{word}")]
        public async Task<IActionResult> DeleteWord(string word)
        {
            try
            {
                await checkersService.DeleteOffensiveWordAsync(word);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return null;
        }

        [HttpPost("check")]
        public async Task<List<string>> CheckReviews([FromBody] List<Review> reviews)
        {
            return await checkersService.RunAutoCheck(reviews);
        }

        [HttpPost("checkOne")]
        public async Task<IActionResult> CheckOneReview([FromBody] Review review)
        {
            try
            {
                await checkersService.RunAICheckForOneReviewAsync(review);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return null;
        }
    }
}
