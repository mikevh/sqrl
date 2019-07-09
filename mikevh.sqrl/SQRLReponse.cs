using System;
using System.Linq;

namespace mikevh.sqrl
{
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

        public string Serialize()
        {
            if(string.IsNullOrEmpty(qry))
            {
                throw new InvalidOperationException("qry parameter required in a response");
            }

            var rv = GetType()
                .GetProperties()
                .Select(x => (key: x.Name, value: x.GetValue(this)?.ToString()))
                .Where(x => !string.IsNullOrEmpty(x.value))
                .Select(x => $"{x.key}={x.value}\r\n")
                .Aggregate("", (r, v) => r + v);
                
            return SQRL.ToBase64URL(rv.UTF8Bytes());
        }

        [Flags]
        public enum TIF
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
}