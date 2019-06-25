using System;
using System.Collections.Generic;
using System.Text;
using mikevh.sqrl;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var responseBody = "dmVyPTENCm51dD1HVmJjcWVjN0VWSkYNCnRpZj01DQpxcnk9L2NsaS5zcXJsP3g9MSZudXQ9R1ZiY3FlYzdFVkpGDQpzdWs9WFBjRXp5cDk5aE5FN2VGTHV3NGpORzdjY0dhb0drUm9kbzBnWkZqYXpTSQ0K";

            var fixedBase64 = SQRL.FixBase64(responseBody);

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(fixedBase64));

            var unpacked = SQRL.Unpack(decoded);

            var expected = new Dictionary<string, string>
            {
                {"ver","1"},
                {"nut","GVbcqec7EVJF" },
                {"tif","5"},
                {"qry", "/cli.sqrl?x=1&nut=GVbcqec7EVJF" },
                {"suk", "XPcEzyp99hNE7eFLuw4jNG7ccGaoGkRodo0gZFjazSI" }
            };


            Assert.Pass();
        }
    }
}