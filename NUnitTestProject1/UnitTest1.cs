using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NUnit.Framework;
using mikevh.sqrl;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void GenKeys()
        {

            var msg = "hello";
            var msgBytes = Encoding.UTF8.GetBytes(msg);

            var seed = Encoding.UTF8.GetBytes("767db4c4-d792-4fed-8f65-c7307662");

            var kp = Sodium.PublicKeyAuth.GenerateKeyPair(seed);

            var priv = Convert.ToBase64String(kp.PrivateKey);
            var pub = Convert.ToBase64String(kp.PublicKey);

            var expPriv = "NzY3ZGI0YzQtZDc5Mi00ZmVkLThmNjUtYzczMDc2NjKGf9nRyrjAJ/O2WYYmX6h0f4AoE2aZgbvuG9bvbRcAGA==";
            var expPub = "hn/Z0cq4wCfztlmGJl+odH+AKBNmmYG77hvW720XABg=";

            var sigBytes = Sodium.PublicKeyAuth.SignDetached(msg, kp.PrivateKey);
            var sig = Convert.ToBase64String(sigBytes);

            Assert.AreEqual(expPriv, priv);
            Assert.AreEqual(expPub, pub);
        }

        [Test]
        public void VerifyDetached()
        {
            //client=dmVyPTENCmNtZD1pZGVudA0KaWRrPXhZdjhvRzY2RE1HWEs4VENXQjU0QkFPcTk5LWFDcEZiMEw2NGE5b3JBcWsNCm9wdD1jcHN-c3VrDQo&server=dmVyPTENCm51dD1HdWZJYkQzVVhlMVpGYkpxdVNOQ2ZBDQp0aWY9NQ0KcXJ5PS9zcXJsP251dD1HdWZJYkQzVVhlMVpGYkpxdVNOQ2ZBDQpzdWs9dVUwUG1rRnpDVFMyUjkxOWpfQ3VKb0EtR0h2T0lvN3JqZ0Q3QUhVTVptNA0K&ids=xvbYlKjPU_bVkSacpXakcJhJX6ZaiPa8cX9wGtVbeEHROddvmQ5ec-xDuMmuPg4-p7Os-PDTO8ZJC12oW-48CQ

            var ids = "xvbYlKjPU_bVkSacpXakcJhJX6ZaiPa8cX9wGtVbeEHROddvmQ5ec-xDuMmuPg4-p7Os-PDTO8ZJC12oW-48CQ";
            var client = "dmVyPTENCmNtZD1pZGVudA0KaWRrPXhZdjhvRzY2RE1HWEs4VENXQjU0QkFPcTk5LWFDcEZiMEw2NGE5b3JBcWsNCm9wdD1jcHN-c3VrDQo";
            var server = "dmVyPTENCm51dD1HdWZJYkQzVVhlMVpGYkpxdVNOQ2ZBDQp0aWY9NQ0KcXJ5PS9zcXJsP251dD1HdWZJYkQzVVhlMVpGYkpxdVNOQ2ZBDQpzdWs9dVUwUG1rRnpDVFMyUjkxOWpfQ3VKb0EtR0h2T0lvN3JqZ0Q3QUhVTVptNA0K";

            var idk = "xYv8oG66DMGXK8TCWB54BAOq99-aCpFb0L64a9orAqk";
            var msg = client + server;

            var msgBytes = Encoding.UTF8.GetBytes(msg);
            var sigBytes = SQRL.FromBase64URL(ids);
            var keyBytes = SQRL.FromBase64URL(idk);

            var result = Sodium.PublicKeyAuth.VerifyDetached(sigBytes, msgBytes, keyBytes);

            Assert.IsTrue(result);
        }

        [Test]
        public void Hex_to_tiff()
        {
            var hexValue = "E0";
            var intVal = (SQRLReponse.TIF)int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            var hasClientFailure = (intVal & SQRLReponse.TIF.client_failure) != 0;
            var hasCommandFaild = (intVal & SQRLReponse.TIF.command_failed) != 0;
            var hasTransientError = (intVal & SQRLReponse.TIF.transient_error) != 0;

            Assert.IsTrue(hasClientFailure);
            Assert.IsTrue(hasCommandFaild);
            Assert.IsTrue(hasTransientError);
        }

        [Test]
        public void TIFF_to_hex()
        {
            var value = SQRLReponse.TIF.client_failure | SQRLReponse.TIF.command_failed | SQRLReponse.TIF.transient_error;

            var hex = value.ToString("X").TrimStart('0');

            Assert.AreEqual("E0", hex);
        }

        [Test]
        public void EmptyQryParamthrows()
        {
            var resp = new SQRLReponse();

            Assert.Throws<InvalidOperationException>(() =>
            {
                resp.Serialize();
            });
        }
        
        class TestClass1
        {
            public string a { get; set; }
            public string c { get; set; }
        }
        
        [Test]
        public void FromBase64URLWithoutPadding_2_equals_padding_needed()
        {
            var input = "YQ";
            var result = SQRL.FromBase64URL(input);

            Assert.AreEqual("a", result);
        }

        [Test]
        public void FromBase64URLWithoutPadding_1_equals_padding_needed()
        {
            var input = "YWI";

            var result = SQRL.FromBase64URL(input);

            Assert.AreEqual("ab", result);
        }

        [Test]
        public void FromBase64URLWithoutPadding_0_equals_padding_needed()
        {
            var input = "YWJj";

            var result = SQRL.FromBase64URL(input);

            Assert.AreEqual("abc", result);
        }
    }
}