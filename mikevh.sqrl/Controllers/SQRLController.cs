using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using mikevh.sqrl.Repos;

namespace mikevh.sqrl.Controllers
{
    public class SQRLController : Controller
    {
        private readonly IUserRepo _users;
        private readonly IMemoryCache _memoryCache;

        public SQRLController(IUserRepo users, IMemoryCache memoryCache)
        {
            _users = users;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public IActionResult Auth(AuthVM auth, string nut)
        {
            var client = SQRL.FormURLDecodeParameterList(auth.Client);

            client.TryGetValue("idk", out var idk);
            client.TryGetValue("suk", out var suk);
            client.TryGetValue("vuk", out var vuk);

            var signature = SQRL.FromBase64URLWithoutPadding(auth.Ids);
            var key = SQRL.FromBase64URLWithoutPadding(idk);
            var valid = SQRL.ValidateSQRLPost(signature, auth.Client, auth.Server, key) ;

            if (!valid) {
                Console.WriteLine("!!! INVALID");
                var rv = "ver=1\r\ntif=40\r\n";
                rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));
                return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
            }

            switch (client["cmd"])
            {
                case "query":
                    return Query(idk);
                case "ident":
                    return Ident(idk, suk, vuk);
                default:
                    return Unauthorized();
            }
        }

        private IActionResult Query(string idk)
         {
            Console.WriteLine("QUERY");

            var nut = SQRL.MakeNut(RequestIP);

            var user = _users.Get(idk);
            var qry = "/sqrl/auth?nut=" + nut;
            var tif = user == null ? "4" : "5";

            Console.WriteLine("QUERY RESPONSE");
            var rv = $"ver=1\r\nnut={nut}\r\ntif={tif}\r\nqry={qry}\r\n";
            Console.WriteLine(rv);

            rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));
            
            return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
        }

        private ContentResult Ident(string idk, string suk, string vuk)
        {
            Console.WriteLine("IDENT");

            var user = _users.Get(idk);

            if (user == null)
            {
                Console.WriteLine("creating user " + idk);
                user = new User
                {
                    idk = idk,
                    suk = suk,
                    vuk = vuk
                };
                _users.Add(user);
            }
            var nut = SQRL.MakeNut(RequestIP);
            _memoryCache.Set(nut, user);

            var rv = $"ver=1\r\nnut={nut}\r\ntif=4\r\nqry=/sqrl/auth?nut={nut}\r\nurl={Request.Scheme}://{Request.Host}/account/login?nut={nut}\r\n";
            rv = SQRL.ToBase64URLWithoutPadding(Encoding.UTF8.GetBytes(rv));
            return new ContentResult { Content = rv, StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };
        }

        private string RequestIP => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";
    }

    public class AuthVM
    {
        public string Client { get; set; }
        public string Server { get; set; }
        public string Ids { get; set; }
    }
}
