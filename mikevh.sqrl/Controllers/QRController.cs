using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;

namespace mikevh.sqrl.Controllers
{
    public class QRController : Controller
    {
        [HttpGet("/qr")]
        public IActionResult QRCode(string nut)
        {
            var link = $"sqrl://10.1.1.2:44343/{SQRL.LoginLink(RequestIP, nut)}";
            var bytes = SQRL.QRCode(link, 90);
            return File(bytes, "image/jpeg");
        }

        private string RequestIP => Request.IsHttps ? Request.Host.Host == "localhost" ? "127.0.0.1" : Request.Host.Host : "0.0.0.0";
    }
}
