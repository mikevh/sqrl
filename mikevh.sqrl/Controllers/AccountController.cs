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
using Microsoft.Extensions.Logging;

namespace mikevh.sqrl.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public AccountController(IMemoryCache memoryCache, ILogger<AccountController> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CPS(string nut)
        {
            _logger.LogTrace("logging IN WITH NUT " + nut);
            var cpsKey = "CPS" + nut;
            _memoryCache.TryGetValue(cpsKey, out User user);

            if (user == null)
            {
                return Unauthorized();
            }
            _memoryCache.Remove(cpsKey);

            var claims = new List<Claim>
            {
                new Claim("idk", user.idk),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);

            // todo: how to get the sqrloption url after login?
            // application level issue

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
