using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSO.WebTest.Models;

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
