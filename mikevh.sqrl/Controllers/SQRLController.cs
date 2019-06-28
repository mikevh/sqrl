using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace mikevh.sqrl.Controllers
{
    public class SQRLController : Controller
    {
        static Dictionary<string, User> _users = new Dictionary<string, User>();
        static Dictionary<string, User> _loggedInNuts = new Dictionary<string, User>();
        public IActionResult See(string input = "")
        {
            try
            {
                var split = input
                    .Trim()
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var line = x.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        var rv = new KeyValuePair<string, string>(
                            line[0],
                            Encoding.UTF8.GetString(SQRL.FromBase64URLWithoutPadding(line[1]))
                        );

                        return rv;
                    }).ToList();
                input = JsonConvert.SerializeObject(split, Formatting.Indented);
            }
            catch
            {
                input = "Error decoding";
            }

            return View(nameof(See), input);
        }

        [HttpPost]
        public IActionResult Auth(AuthVM auth, string nut)
        {
            var client = SQRL.FormURLDecodeParameterList(auth.Client);
            var signature = SQRL.FromBase64URLWithoutPadding(auth.Ids);
            var key = SQRL.FromBase64URLWithoutPadding(client["idk"]);
            var valid = SQRL.ValidateSQRLPost(signature, auth.Client, auth.Server, key);
            
            if(!valid) {
                Console.WriteLine("!!! INVALID");
                var rv = "ver=1\r\ntif=40\r\n";
                rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));
                return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
            }

            switch (client["cmd"])
            {
                case "query":
                    return Query(client["idk"]);
                case "ident":
                    return Ident(client["idk"], client["suk"], client["vuk"]);
                default:
                    return Unauthorized();
            }
        }

        private IActionResult Query(string idk)
        {
            Console.WriteLine("QUERY");
            var n = SQRL.MakeNut();

            var known = _users.ContainsKey(idk);
            var qry = "/sqrl/auth?nut=" + n;

            if (known)
            {
                var user = _users[idk];
                _loggedInNuts.Add(n, user);
                qry = "/sqrl/loggedin?nut=" + n;
            }

            var tif = known ? SQRL.TIF.id_match.ToString("X").TrimStart('0') : "0";

            var rv = $"ver=1\r\nnut={n}\r\ntif={tif}\r\nqry={qry}\r\n";

            rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));

            Console.WriteLine("QUERY RESPONSE");
            Console.WriteLine(rv);

            return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
        }

        private ContentResult Ident(string idk, string suk, string vuk)
        {
            Console.WriteLine("IDENT");

            if (!_users.ContainsKey(idk))
            {
                Console.WriteLine("adding user " + idk);
                _users.Add(idk, new User
                {
                    idk = idk,
                    suk = suk,
                    vuk = vuk
                });
            }
            var n = SQRL.MakeNut();
            var user = _users[idk];
            _loggedInNuts.Add(n, user);

            var rv = $"ver=1\r\nnut={n}\r\ntif=4\r\nqry=/sqrl/auth?nut={n}\r\nurl={Request.Scheme}://{Request.Host}/sqrl/loggedin?nut={n}\r\n";
            rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));
            return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
        }

        [HttpGet]
        public async Task<IActionResult> LoggedIn(string nut)
        {
            Console.WriteLine("LOGGED IN WITH NUT " + nut);
            if(!_loggedInNuts.ContainsKey(nut))
            {
                return Unauthorized();
            }
            var user = _loggedInNuts[nut];
            _loggedInNuts.Remove(nut);

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

    public class User
    {
        public string idk { get; set; }
        public string suk { get; set; }
        public string vuk { get; set; }
    }

    public class AuthVM
    {
        public string Client { get; set; }
        public string Server { get; set; }
        public string Ids { get; set; }
    }
}
