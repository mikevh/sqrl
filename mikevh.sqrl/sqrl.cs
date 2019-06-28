using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Sodium; 

namespace mikevh.sqrl
{
    public static class SQRL
    {
        public static string LoginLink => "sqrl/auth?nut=" + MakeNut();

        public static string MakeNut()
        {
            var bytes = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(bytes);

            return ToBase64URLWithoutPadding(bytes);
        }

        public static Dictionary<string,string> FormURLDecodeParameterList(string input) => Unpack(Encoding.UTF8.GetString(FromBase64URLWithoutPadding(input)));

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

        public static string ToBase64URLWithoutPadding(byte[] input)
        {
            return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static byte[] FromBase64URLWithoutPadding(string input)
        {
            if(input.Length % 4 > 0)
            {
                switch(input.Length % 4)
                {
                    case 2: input += "=="; break;
                    case 3: input += "="; break;
                }
            }
            input = input.Replace('-', '+').Replace('_', '/');

            var rv = Convert.FromBase64String(input);

            return rv;
        }

        public static bool ValidateSQRLPost(byte[] signature, string client, string server, byte[] key)
        {
            return PublicKeyAuth.VerifyDetached(signature, Encoding.UTF8.GetBytes(client + server), key);
        }

        public static string URLSafeBase64(string input) => input.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        [Flags]
        public enum TIF : int
        {
            id_match = 0x01,
            previous_id_match = 0x02,
            ips_match = 0x04,
            sqrl_disabled = 0x08,
            function_not_supported = 0x10,
            transient_error = 0x20,
            command_failed = 0x40,
            client_failure = 0x80,
            bad_id_association = 0x100
        }
    }

    public class SQRLReponse
    {
        public int ver { get; set; } = 1;
        public string nut { get; set; }
        public string tif { get; set; }
        public string qry { get; set; }
        public string url { get; set; } // required for responses to non-query requests
        public string sin { get; set; }
        public string suk { get; set; }
        public string ask { get; set; }
        public string can { get; set; }
    }

    public class SQRLRequest
    {
        public int ver { get; set; }
        public string cmd { get; set; }
        public string opt { get; set; }
        public int btn { get; set; }
        public string qry { get; set; }
        public string idk { get; set; }
        public string pikd { get; set; }
        public string suk { get; set; }
        public string vuk { get; set; }
        public string ins { get; set; }
        public string pins { get; set; }
        public string ids { get; set; }
        public string pids { get; set; }
        public string urs { get; set; }

        public enum Options
        {
            noiptest,
            sqrlonly,
            hardlock,
            cps,
            suk
        }
    }
}
