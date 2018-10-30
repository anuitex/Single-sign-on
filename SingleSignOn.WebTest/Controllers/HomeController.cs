using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SingleSignOn.WebTest.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index(string token = null, string redirectUrl = null)
        {
            return View();
        }
    }
}
