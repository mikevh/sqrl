using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using mikevh.sqrl.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mikevh.sqrl.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMemoryCache _memoryCache;

        public AccountController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string nut)
        {
            Console.WriteLine("LOGGED IN WITH NUT " + nut);

            _memoryCache.TryGetValue(nut, out User user);

            if (user == null)
            {
                return Unauthorized();
            }
            _memoryCache.Remove(nut);

            var claims = new List<Claim>
            {
                new Claim("idk", user.idk),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);

            return RedirectToAction("Hello", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
