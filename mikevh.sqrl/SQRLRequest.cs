namespace mikevh.sqrl
{
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

        public byte[] Signature { get; set; }
        public byte[] PublicKey { get; set; }

        public bool IsValid { get; set; }
        public string RequestIP { get; set; }
        public string Host { get; set; }


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