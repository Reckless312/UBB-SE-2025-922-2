using System.Diagnostics;
using DataAccess.AutoChecker;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class AdminController : Controller
    {
        private IReviewService reviewService;
        private IUpgradeRequestsService upgradeRequestService;
        private ICheckersService checkersService;
        private IUserService userService;
        private IAutoCheck autoCheckService;
        public AdminController(IReviewService newReviewService, IUpgradeRequestsService newUpgradeRequestService, IRolesService newRolesService,
            ICheckersService newCheckersService, IAutoCheck autoCheck, IUserService userService)
        {
            this.reviewService = newReviewService;
            this.upgradeRequestService = newUpgradeRequestService;
            this.checkersService = newCheckersService;
            this.autoCheckService = autoCheck;
            this.userService = userService;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            IEnumerable<Review> reviews = await this.reviewService.GetFlaggedReviews();
            IEnumerable<UpgradeRequest> upgradeRequests = await this.upgradeRequestService.RetrieveAllUpgradeRequests();
            IEnumerable<string> offensiveWords = await this.checkersService.GetOffensiveWordsList();
            List<User> users = await this.userService.GetAllUsers();
            IEnumerable<User> appealeadUsers = users.Where(user => user.HasSubmittedAppeal && user.AssignedRole == RoleType.Banned);

            AdminDashboardViewModel adminDashboardViewModel = new AdminDashboardViewModel()
            {
                Reviews = reviews,
                UpgradeRequests = upgradeRequests,
                OffensiveWords = offensiveWords,
                AppealsList = appealeadUsers
            };

            List<AppealDetailsViewModel> appealsWithDetails = new List<AppealDetailsViewModel>();

            foreach (User user in appealeadUsers)
            {
                List<Review> userReviews = await this.reviewService.GetReviewsByUser(user.UserId);
                appealsWithDetails.Add(new AppealDetailsViewModel()
                {
                    User = user,
                    Reviews = userReviews
                });
            }
            adminDashboardViewModel.AppealsWithDetails = appealsWithDetails;

            return View(adminDashboardViewModel);
        }

        public async Task<IActionResult> AcceptReview(int reviewId)
        {
            await this.reviewService.ResetReviewFlags(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> HideReview(int reviewId)
        {
            await this.reviewService.HideReview(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> AICheckReview(int reviewId)
        {
            try
            {
                Review? review = await this.reviewService.GetReviewById(reviewId);

                if (review == null)
                {
                    ViewBag.ErrorMessage = "Review not found. Please try again.";
                    return RedirectToAction("AdminDashboard");
                }

                this.checkersService.RunAICheckForOneReviewAsync(review);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Couldn't run AiChecker. Make sure you have your token set correctly:", exception.Message);
            }
            return RedirectToAction("AdminDashboard");
        }
        public async Task<IActionResult> AutomaticallyCheckReviews()
        {
            List<Review> reviews = await this.reviewService.GetFlaggedReviews();
            List<string> messages = await Task.Run(() => this.checkersService.RunAutoCheck(reviews));

            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> Accept(int id)
        {
            await this.upgradeRequestService.ProcessUpgradeRequest(true, id);
            await this.upgradeRequestService.RemoveUpgradeRequestByIdentifier(id);
            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> Decline(int id)
        {
            await this.upgradeRequestService.ProcessUpgradeRequest(false, id);
            await this.upgradeRequestService.RemoveUpgradeRequestByIdentifier(id);
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AddOffensiveWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                await this.checkersService.AddOffensiveWordAsync(word);
            }
            return Json(await this.checkersService.GetOffensiveWordsList());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOffensiveWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                await this.checkersService.DeleteOffensiveWordAsync(word);
            }
            return Json(await this.checkersService.GetOffensiveWordsList());
        }

        [HttpPost]
        public async Task<IActionResult> AcceptAppeal(Guid userId)
        {
            User? user = await this.userService.GetUserById(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found. Please try again.";
                return RedirectToAction("AdminDashboard");
            }

            user.AssignedRole = RoleType.User;
            user.HasSubmittedAppeal = false;
            await this.userService.UpdateUser(user);

            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> KeepBan(Guid userId)
        {
            User? user = await this.userService.GetUserById(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found. Please try again.";
                return RedirectToAction("AdminDashboard");
            }

            user.HasSubmittedAppeal = false;
            await this.userService.UpdateUser(user);

            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CloseAppealCase(Guid userId)
        {
            User? user = await this.userService.GetUserById(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found. Please try again.";
                return RedirectToAction("AdminDashboard");
            }

            user.HasSubmittedAppeal = false;
            await this.userService.UpdateUser(user);

            return RedirectToAction("AdminDashboard");
        }
    }
}