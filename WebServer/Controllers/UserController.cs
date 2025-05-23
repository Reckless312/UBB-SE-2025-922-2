using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Model.Authentication;
using ServerAPI.Data;
using DataAccess.Model.AdminDashboard;
using IRepository;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class UserController : Controller
    {

        IUserService userService;
        IReviewService reviewService;
        public UserController( IUserService userServ, IReviewService reviewServ)
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
                currentUserDrinks = new List<string>() { "beer", "lemonade", "vodka"}
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

    }
}
