using Microsoft.AspNetCore.Mvc;

namespace SSO.API.Controllers
{
    public class ErrorController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
