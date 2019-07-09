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
        private readonly IUserRepo _userRepo;
        private readonly IMemoryCache _memoryCache;

        public SQRLController(IUserRepo userRepo, IMemoryCache memoryCache)
        {
            _userRepo = userRepo;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public IActionResult Auth(SQRLVM auth, string nut)
        {
            var req = SQRL.DecodeRequest(Request.Host.Value, RequestIP(), auth);
            var res = SQRL.ComoseResponse(req, _userRepo.Get, _userRepo.Update, (key, user) =>
            {
                _memoryCache.Set(key, user);
            });

            return new ContentResult { Content = res.Serialize(), StatusCode = 200, ContentType = "application/x-www-form-urlencoded" };

            string RequestIP() => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";
        }
    }
}
