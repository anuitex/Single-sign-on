using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SingleSignOn.API.Areas.Home
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}