using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View("~/Views/Admin/AdminDashboard.cshtml");
        }
    }
}