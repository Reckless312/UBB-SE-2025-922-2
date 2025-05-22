using DataAccess.Model.AdminDashboard;
using DataAccess.Model.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class AdminController : Controller
    {
        private readonly IReviewService reviewService;
        private readonly IUpgradeRequestsService upgradeRequestService;
        private readonly IOffensiveWordsRepository offensiveWordsService;
        private readonly ICheckersService checkersService;
        public AdminController(IReviewService reviewService, IUpgradeRequestsService upgradeRequestService, IRolesService rolesService, IOffensiveWordsRepository offensiveWordsService)
        {
            this.reviewService = reviewService;
            this.upgradeRequestService = upgradeRequestService;
            this.offensiveWordsService = offensiveWordsService;
        }

        public IActionResult AdminDashboard()
        {
            IEnumerable<Review> reviews = reviewService.GetFlaggedReviews().Result;
            IEnumerable<UpgradeRequest> upgradeRequests = upgradeRequestService.RetrieveAllUpgradeRequests().Result;
            IEnumerable<string> offensiveWords = offensiveWordsService.LoadOffensiveWords().Result;
            AdminDashboardViewModel adminDashboardViewModel = new AdminDashboardViewModel()
            {
                Reviews = reviews,
                UpgradeRequests = upgradeRequests,
                OffensiveWords = offensiveWords
            };
            return View(adminDashboardViewModel);
        }

        public IActionResult AcceptReview(int reviewId)
        {
            reviewService.ResetReviewFlags(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult HideReview(int reviewId)
        {
            reviewService.HideReview(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult AICheckReview(int reviewId)
        {
            try
            {
                checkersService.RunAICheckForOneReviewAsync(reviewService.GetReviewById(reviewId).Result);
            }
            catch (Exception exception) {
                Debug.WriteLine("Couldn't run AiChecker. Make sure you have your token set correctly.");
            }
            return RedirectToAction("AdminDashboard");
        }
    }
}