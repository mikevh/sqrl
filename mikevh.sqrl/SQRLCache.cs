//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;

//namespace mikevh.sqrl
//{
//    public interface ISQRLCache
//    {
//        void LinkNut(string idk, string prevNut, string newNut);
//        string LoggedInNut(string nut);
//    }

//    public class SQRLCache : ISQRLCache
//    {
//        private readonly ILogger _logger;
//        private readonly IMemoryCache _memoryCache;

//        private readonly string LINKED = "linked";

//        public SQRLCache(ILogger<SQRLCache> logger, IMemoryCache memoryCache)
//        {
//            _logger = logger;
//            _memoryCache = memoryCache;
//        }

//        public void LinkNut(string idk, string prevNut, string newNut)
//        {
//            _memoryCache.TryGetValue(LINKED + idk, out string val);

//            if(val != null)
//            {

//            }

//            _memoryCache.Set(LINKED + idk, string.Join(",", ));
//        }

//        public string LoggedInNut(string nut)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}