using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class ProfileController : Controller
    {
        private IUserService userService;
        private IReviewService reviewService;
        public ProfileController(IUserService userService, IReviewService reviewService)
        {
            this.userService = userService;
            this.reviewService = reviewService;
        }

        public async Task<IActionResult> UserPage()
        {
            // TO DO: The SET / GET approach for user / session id will probably not work with multiple browser connections
            Guid userId = AuthenticationService.GetCurrentUserId();
            User? currentUser = await this.userService.GetUserById(userId);

            if (currentUser == null)
            {
                ViewBag.ErrorMessage = "User not found. Please log in again.";
                return RedirectToAction("MainWindow", "Auth");
            }

            IEnumerable<Review> reviews = await reviewService.GetReviewsByUser(currentUser.UserId);
            UserPageModel userPageModel = new UserPageModel()
            {
                CurrentUser = currentUser,
                CurrentUserReviews = reviews,
                CurrentUserDrinks = new List<string>() { "beer", "lemonade", "vodka" }
            };
            return View(userPageModel);
        }

        public IActionResult LogOut()
        {
            return RedirectToAction("MainWindow", "Auth");
        }
    }
}