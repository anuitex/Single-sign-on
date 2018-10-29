using Microsoft.AspNetCore.Mvc;

namespace SSO.WebTest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string token = null, string redirectUrl = null)
        {
            return View();
        }
    }
}
