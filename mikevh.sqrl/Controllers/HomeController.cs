using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using mikevh.sqrl.Models;
using mikevh.sqrl.Repos;

namespace mikevh.sqrl.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IMemoryCache _memoryCache;

        public HomeController(IUserRepo userRepo, IMemoryCache memoryCache)
        {
            _userRepo = userRepo;
            _memoryCache = memoryCache;
        }
        
        [AllowAnonymous]
        [HttpGet("sync.txt")]
        public IActionResult Sync()
        {
            return NotFound();
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var nut = SQRL.MakeNut(RequestIP);
            var link = SQRL.LoginLink(RequestIP, nut);

            var vm = new IndexVM
            {
                Nut = nut,
                SQRLLoginLink = $"sqrl://{Request.Host}/{link}"
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Hello()
        {
            var idk = User.Claims.First(x => x.Type == "idk").Value;
            var user = _userRepo.Get(idk);

            var vm = new HelloVM
            {
                User = user
            };

            return View(vm);
        }

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

        [HttpGet]
        public IActionResult Ask()
        {
            return View();
        }

        // posted questions
        [HttpPost]
        public IActionResult Ask(AskVM vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }

            var nut = SQRL.MakeNut(RequestIP, false);
            var ask = SQRL.ToBase64URL(string.Join("~", vm.Question, vm.Button1, vm.Button2).UTF8Bytes());

            // stash nut with what the questions are
            _memoryCache.Set("ask" + nut, ask);

            // send them to a page with the sqrl prompt for this nut
            return RedirectToAction(nameof(Prompt), new { nut });
        }

        [HttpGet]
        public IActionResult QRCode(string url)
        {
            var b = new byte[] { 0 };

            return File(b, "image/jpeg");
        }

        // render the link with the supplied nut
        [HttpGet]
        public IActionResult Prompt(string nut)
        {

            var vm = new PromptVM
            {
                AskLink = $"sqrl://{Request.Host}/{SQRL.LoginLink(RequestIP, nut)}"
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string RequestIP => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";
    }

    public class PromptVM
    {
        public string AskLink { get; set; }
        public string EncodedAskLink => SQRL.ToBase64URL(AskLink.UTF8Bytes());
    }

    public class AskVM
    {
        [Required]
        public string Question { get; set; }
        [Required]
        [DisplayName("Button 1")]
        public string Button1 { get; set; }
        [Required]
        [DisplayName("Button 2")]
        public string Button2 { get; set; }
    }

    public class HelloVM
    {
        public User User { get; set; }
    }

    public class IndexVM
    {
        public string Nut { get; set; }
        public string SQRLLoginLink { get; set; }
        public string EncodedSQRLURL => SQRL.ToBase64URL(SQRLLoginLink.UTF8Bytes());
        public string QRCodeImageSrc => "/qr?nut=" + Nut;
    }
}
