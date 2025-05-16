using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult MainWindow()
        {
            return View();
        }

        public IActionResult TwoFactorAuthSetup()
        {
            return View();
        }

        public IActionResult TwoFactorAuthCheck()
        {
            return View();
        }
    }
}