using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace mikevh.sqrl.Controllers
{
    public class SQRLController : Controller
    {
        static List<string> _knownIds = new List<string>();

        [HttpPost]
        public IActionResult Auth(AuthVM auth, string nut)
        {
            var client = SQRL.Unpack(Encoding.UTF8.GetString(Convert.FromBase64String(SQRL.FixBase64(auth.Client))));
            var server = Encoding.UTF8.GetString(Convert.FromBase64String(SQRL.FixBase64(auth.Server)));

            // https://www.grc.com/sqrl/semantics.htm
            switch (client["cmd"])
            {
                case "query": return Query(); break;
            }

            return Unauthorized();

            IActionResult Query()
            {
                var response = new Response {tif = SQRL.TIF.ips_match};

                response.qry = "/sqrl/auth?nut=" + response.nut;

                var packed = SQRL.Pack(response);

                return Ok(packed);
            }
        }
    }

    public class Response
    {
        public string ver { get; set; } = "1";
        public string nut { get; set; } = SQRL.Nut;
        public SQRL.TIF tif { get; set; } = SQRL.TIF.client_failure;
        public string qry { get; set; } = "/sqrl";
    }

    public class AuthVM
    {
        public string Client { get; set; }
        public string Server { get; set; }
        public int Ids { get; set; }
    }
}
