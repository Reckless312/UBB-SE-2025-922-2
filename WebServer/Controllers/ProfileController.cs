using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class ProfileController : Controller
    {
        IUserService userService;
        IReviewService reviewService;
        public ProfileController(IUserService userServ, IReviewService reviewServ)
        {
            userService = userServ;
            reviewService = reviewServ;
        }

        public IActionResult Index()
        {
            User currentUser = userService.GetCurrentUser().Result;
            IEnumerable<Review> reviews = reviewService.GetReviewsByUser(currentUser.UserId).Result;
            UserPageModel userPageModel = new UserPageModel()
            {
                currentUser = currentUser,
                currentUserReviews = reviews,
                currentUserDrinks = new List<string>() { "beer", "lemonade", "vodka" }
            };
            return View(userPageModel);
        }
        public IActionResult UserPage()
        {
            User currentUser = userService.GetCurrentUser().Result;
            IEnumerable<Review> reviews = reviewService.GetReviewsByUser(currentUser.UserId).Result;
            UserPageModel userPageModel = new UserPageModel()
            {
                currentUser = currentUser,
                currentUserReviews = reviews,
                currentUserDrinks = new List<string>() { "beer", "lemonade", "vodka" }
            };
            return View(userPageModel);
        }
        public IActionResult LogOut()
        {
            return RedirectToAction("MainWindow", "Auth");
        }
    }
}
