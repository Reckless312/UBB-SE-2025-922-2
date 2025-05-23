using DataAccess.AutoChecker;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Model.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using WebServer.Models;


namespace WebServer.Controllers
{
    public class AdminController : Controller
    {
        private IReviewService reviewService;
        private IUpgradeRequestsService upgradeRequestService;
        private IOffensiveWordsRepository offensiveWordsService;
        private ICheckersService checkersService;
        private IAutoCheck autoCheckService;
        private IUserService userService;
        private IRolesService rolesService;
        public AdminController(IReviewService newReviewService, IUpgradeRequestsService newUpgradeRequestService, IRolesService newRolesService, IOffensiveWordsRepository newOffensiveWordsService, ICheckersService newCheckersService, IAutoCheck autoCheck, IUserService newUserService)
        {
            this.reviewService = newReviewService;
            this.upgradeRequestService = newUpgradeRequestService;
            this.offensiveWordsService = newOffensiveWordsService;
            this.checkersService = newCheckersService;
            this.autoCheckService = autoCheck;
            this.userService = newUserService;
            this.rolesService = newRolesService;
        }

      


        public IActionResult AdminDashboard()
        {

            IEnumerable<Review> reviews = this.reviewService.GetFlaggedReviews().Result;
            IEnumerable<UpgradeRequest> upgradeRequests = this.upgradeRequestService.RetrieveAllUpgradeRequests().Result;
            IEnumerable<string> offensiveWords = this.offensiveWordsService.LoadOffensiveWords().Result;
            //AdminDashboardViewModel adminDashboardViewModel = new AdminDashboardViewModel()




            var allUsers = this.userService.GetAllUsers().Result;
            var appealsList = allUsers.Where(u => u.HasSubmittedAppeal && u.AssignedRole == RoleType.Banned);
            var model = new AdminDashboardViewModel
            {
                Reviews = reviews,
                UpgradeRequests = upgradeRequests,
                OffensiveWords = offensiveWords,
                AppealsList = appealsList
            };

            //  Attach reviews to each appealed user
            var appealsWithDetails = new List<AppealDetailsViewModel>();
            foreach (var user in appealsList)
            {
                var userReviews = this.reviewService.GetReviewsByUser(user.UserId).Result;
                appealsWithDetails.Add(new AppealDetailsViewModel { User = user, Reviews = userReviews });
            }
            model.AppealsWithDetails = appealsWithDetails;

            return View(model);
        }




        public IActionResult AcceptReview(int reviewId)
        {
            this.reviewService.ResetReviewFlags(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult HideReview(int reviewId)
        {
            this.reviewService.HideReview(reviewId);
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult AICheckReview(int reviewId)
        {
            try
            {
                this.checkersService.RunAICheckForOneReviewAsync(this.reviewService.GetReviewById(reviewId).Result);
            }
            catch (Exception exception) {
                Debug.WriteLine("Couldn't run AiChecker. Make sure you have your token set correctly:", exception.Message);
            }
            return RedirectToAction("AdminDashboard");
        }
        public IActionResult AutomaticallyCheckReviews()
        {
            foreach(Review review in reviewService.GetFlaggedReviews().Result)
                if(this.autoCheckService.AutoCheckReview(review.Content))
                    this.reviewService.HideReview(review.ReviewId);

            return RedirectToAction("AdminDashboard");
        }

        public IActionResult Accept(int id)
        {
            this.upgradeRequestService.ProcessUpgradeRequest(true, id).Wait();
            this.upgradeRequestService.RemoveUpgradeRequestByIdentifier(id).Wait();
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult Decline(int id)
        {
            this.upgradeRequestService.ProcessUpgradeRequest(false, id).Wait();
            this.upgradeRequestService.RemoveUpgradeRequestByIdentifier(id).Wait();
            return RedirectToAction("AdminDashboard");
        }


        [HttpPost]
        public IActionResult AcceptAppeal(Guid userId)
        {
            var user = this.userService.GetUserById(userId).Result;
            if (user == null) return NotFound();

            user.AssignedRole      = RoleType.User;
            user.HasSubmittedAppeal = false;         
            this.userService.UpdateUser(user).Wait();

            return RedirectToAction("AdminDashboard");
        }


        [HttpPost]
        public IActionResult KeepBan(Guid userId)
        {
            var user = this.userService.GetUserById(userId).Result;
            if (user == null) return NotFound();

            user.HasSubmittedAppeal = false;                     
            this.userService.UpdateUser(user).Wait();

            return RedirectToAction("AdminDashboard");
        }



        [HttpPost]
        public IActionResult CloseAppealCase(Guid userId)
        {
            var user = this.userService.GetUserById(userId).Result;
            if (user == null) return NotFound();
            user.HasSubmittedAppeal = false;
            this.userService.UpdateUser(user).Wait();
            return RedirectToAction("AdminDashboard");
        }


    }
}