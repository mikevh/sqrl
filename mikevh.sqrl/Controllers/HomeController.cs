using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mikevh.sqrl.Models;

namespace mikevh.sqrl.Controllers
{
    public class HomeController : Controller
    {
        private string RequestIP => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";

        public IActionResult Index()
        {
            var model = new IndexVM
            {
                SQRLLoginLink = $"sqrl://{Request.Host}/{SQRL.LoginLink(RequestIP)}"
            };

            return View(model);
        }

        [Authorize]
        public IActionResult Hello()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class IndexVM
    {
        public string SQRLLoginLink { get; set; }
        public string EncodedSQRLURL => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SQRLLoginLink));
    }
}
