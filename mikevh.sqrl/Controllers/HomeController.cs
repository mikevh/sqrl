using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mikevh.sqrl.Models;
using mikevh.sqrl.Repos;

namespace mikevh.sqrl.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepo _userRepo;

        public HomeController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        
        public IActionResult Index()
        {
            var vm = new IndexVM
            {
                SQRLLoginLink = $"sqrl://{Request.Host}/{SQRL.LoginLink(RequestIP)}"
            };

            return View(vm);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Hello()
        {
            var idk = User.Claims.First(x => x.Type == "idk").Value;
            var user = _userRepo.Get(idk) as User;

            var vm = new HelloVM
            {
                User = user
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Hello(HelloVM vm)
        {
            var idk = User.Claims.First(x => x.Type == "idk").Value;
            var user = _userRepo.Get(idk);

            user.Name = vm.User.Name;
            user.UpdateCount++;

            _userRepo.Update(user);

            return Hello();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string RequestIP => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";
    }

    public class HelloVM
    {
        public User User { get; set; }
    }

    public class IndexVM
    {
        public string SQRLLoginLink { get; set; }
        public string EncodedSQRLURL => SQRL.ToBase64URL(SQRLLoginLink.UTF8Bytes());
    }
}
