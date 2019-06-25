using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using mikevh.sqrl.Controllers;

namespace mikevh.sqrl
{
    public static class SQRL
    {
        public static string LoginLink => "sqrl/auth?nut=" + Nut;

        public static string Nut
        {
            get
            {
                var bytes = new byte[16];
                new RNGCryptoServiceProvider().GetBytes(bytes);

                return Convert.ToBase64String(bytes);
            }
        }

        public static Dictionary<string,string> Unpack(string input)
        {
            var rv = new Dictionary<string,string>();
            var list = input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in list)
            {
                var idx = s.IndexOf('=');
                rv.Add(s.Substring(0, idx), s.Substring(idx+1));
            }

            return rv;
        }

        public static string FixBase64(string input)
        {
            input = input.Replace('_', '/').Replace('-', '+');
            switch (input.Length % 4)
            {
                case 2: input += "==";
                    break;
                case 3: input += "=";
                    break;
            }
            return input;
        }

        public static string URLSafeBase64(string input) => input.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        [Flags]
        public enum TIF
        {
            current_id_match = 0x01,
            previous_id_match = 0x02,
            ips_match = 0x04,
            sqrl_disabled = 0x08,
            function_not_supported = 0x10,
            transient_error = 0x20,
            command_failed = 0x40,
            client_failure = 0x80,
            bad_id_association = 0x100
        }

        public static string Pack(Response response)
        {
            var kvps = typeof(Response).GetProperties().Select(x => $"{x.Name}={x.GetValue(response)}");
            var rv = string.Join("&", kvps);
            rv = Convert.ToBase64String(Encoding.UTF8.GetBytes(rv));
            return URLSafeBase64(rv);
        }
    }
}
