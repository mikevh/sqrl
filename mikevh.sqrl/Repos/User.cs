using System;

namespace mikevh.sqrl.Repos
{
    public class User
    {
        public string idk { get; set; }
        public string suk { get; set; }
        public string vuk { get; set; }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public int LoginCount { get; set; }
        public int UpdateCount { get; set; }
    }
}