using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    public class SuccessController : Controller
    {
        public IActionResult SuccessPage()
        {
            return View();
        }
    }
}
