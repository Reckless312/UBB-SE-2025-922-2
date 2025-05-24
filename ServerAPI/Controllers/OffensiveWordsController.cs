using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Model.AdminDashboard;
using DataAccess.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/offensiveWords")]
    public class OffensiveWordsController : ControllerBase
    {
        private readonly ICheckersService checkersService;

        public OffensiveWordsController(ICheckersService checkersService)
        {
            this.checkersService = checkersService;
        }

        [HttpGet]
        public async Task<List<OffensiveWord>> GetAllWords()
        {
            HashSet<string> words = await this.checkersService.GetOffensiveWordsList();
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
                await this.checkersService.AddOffensiveWordAsync(word.Word);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpDelete("delete/{word}")]
        public async Task<IActionResult> DeleteWord(string word)
        {
            try
            {
                await this.checkersService.DeleteOffensiveWordAsync(word);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpPost("check")]
        public async Task<List<string>> CheckReviews([FromBody] List<Review> reviews)
        {
            return await this.checkersService.RunAutoCheck(reviews);
        }

        [HttpPost("checkOne")]
        public IActionResult CheckOneReview([FromBody] Review review)
        {
            try
            {
                this.checkersService.RunAICheckForOneReviewAsync(review);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
