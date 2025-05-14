using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    public class UserController : Controller
    {
        public IActionResult UserPage()
        {
            return View();
        }
    }
}
