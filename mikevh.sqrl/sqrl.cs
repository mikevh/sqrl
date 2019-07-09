using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using mikevh.sqrl.Repos;
using Sodium; 

namespace mikevh.sqrl
{
    public static class SQRL
    {
        private static uint _nutCounter = 0;

        public static string MakeNut(string ipaddress, bool isURLClick = true)
        {
            var bytes = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(bytes);

            //for(var i=0; i< ipBytes.Length; i++)
            //{
            //    bytes[i] = ipBytes[i];
            //}

            //var time = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //var timeBytes = BitConverter.GetBytes(time);
            //for(var i=0; i<timeBytes.Length; i++)
            //{
            //    bytes[i + 4] = timeBytes[i];
            //}

            //var counterBytes = BitConverter.GetBytes(_nutCounter++);
            //for(var i=0; i<counterBytes.Length; i++)
            //{
            //    bytes[i + 8] = counterBytes[i];
            //}

            // todo: set the last bit based on isURLClick
            
            return ToBase64URL(bytes);
        }

        public static bool ValidateNut(string requestIP, string nut)
        {
            var decodedNutBytes = FromBase64URL(nut);
            var currentIPBytes = System.Net.IPAddress.Parse(requestIP).GetAddressBytes();
            for(var i=0; i<currentIPBytes.Length; i++)
            {
                if(decodedNutBytes[i] != currentIPBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static SQRLRequest DecodeRequest(string host, string ipAddress, SQRLVM vm)
        {
            var dict = Unpack(FromBase64URL(vm.Client).UTF8String());

            var rv = new SQRLRequest
            {
                idk = dict.GetOrDefault("idk"),
                suk = dict.GetOrDefault("suk"),
                vuk = dict.GetOrDefault("vuk"),
                cmd = dict.GetOrDefault("cmd"),

                Signature = FromBase64URL(vm.Ids),
            };

            rv.PublicKey = FromBase64URL(rv.idk);
            rv.IsValid = ValidateSQRLPost(rv.Signature, vm.Client, vm.Server, rv.PublicKey);
            rv.RequestIP = ipAddress;
            rv.Host = host;

            return rv;
        }

        public static Dictionary<string,string> Unpack(string input)
        {
            var rv = new Dictionary<string,string>();
            var list = input.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in list)
            {
                var idx = s.IndexOf('=');
                rv.Add(s.Substring(0, idx), s.Substring(idx+1));
            }

            return rv;
        }

        public static string LoginLink(string ipaddress, bool isURLClick = true) => "sqrl/auth?nut=" + MakeNut(ipaddress, isURLClick);

        public static string UTF8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

        public static byte[] UTF8Bytes(this string input) => Encoding.UTF8.GetBytes(input);

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key) => d.TryGetValue(key, out var v) ? v : default(TValue);

        public static string ToBase64URL(byte[] input) => Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        public static byte[] FromBase64URL(string input)
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

            return Convert.FromBase64String(input);
        }

        public static bool ValidateSQRLPost(byte[] sig, string client, string server, byte[] key)
        {
            return PublicKeyAuth.VerifyDetached(sig, (client + server).UTF8Bytes(), key);
        }

        public static SQRLReponse ComoseResponse(SQRLRequest req, Func<string,User> getUser, Action<User> saveUser, Action<string,User> loggedIn)
        {
            var resp = new SQRLReponse
            {
                nut = MakeNut(req.RequestIP),
            };

            if (!req.IsValid)
            {
                resp.tif = SQRLReponse.TIF.command_failed.ToHex();
                return resp;
            }

            var user = getUser(req.idk);
            resp.tif = (user == null ? SQRLReponse.TIF.ips_match : SQRLReponse.TIF.ips_match | SQRLReponse.TIF.id_match).ToHex();
            resp.qry = "/sqrl/auth?nut=" + resp.nut;

            switch (req.cmd)
            {
                case "query":
                    return resp;
                case "ident":
                    if (user == null)
                    {
                        user = new User
                        {
                            idk = req.idk,
                            suk = req.suk,
                            vuk = req.vuk,
                            CreatedOn = DateTime.Now,
                        };
                    }
                    else
                    {
                        user.LoginCount++;
                        user.LastLoggedIn = DateTime.Now;
                        resp.suk = user.suk;
                    }
                    saveUser(user);
                    loggedIn("CPS" + resp.nut, user);

                    resp.tif = (SQRLReponse.TIF.id_match | SQRLReponse.TIF.ips_match).ToHex();
                    
                    return resp;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
