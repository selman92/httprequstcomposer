using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HTTPRequestComposer.Tests
{
    [TestFixture]
    public class HttpRequestComposerTests
    {
        [Test]
        public void TestParseRawRequest1()
        {
            var composer = new HttpRequestComposer();

            string rawRequest = "GET https://www.google.com/ HTTP/1.1\r\n"  +
                                  "Host: www.google.com\r\n" +
                                  "Connection: keep­alive\r\n" +
                                  "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8\r\n" +
                                  "User-Agent:Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36\r\n" +
                                  "Accept-Encoding: gzip, deflate\r\n" +
                                  "Accept-Language: en­US,en;q=0.8";

            bool result = composer.ParseRawRequest(rawRequest);
            Assert.AreEqual(result, true);
        }

        [Test]
        public void TestParseRawRequest2()
        {
            var composer = new HttpRequestComposer();

            // no space between request method and uri
            string rawRequest = "GEThttps://www.google.com/ HTTP/1.1\r\n" +
                                "Host: www.google.com\r\n" +
                                 "Connection: keep­alive\r\n" +
                                 "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8";

            bool result = composer.ParseRawRequest(rawRequest);
            Assert.AreEqual(result, false);
        }
        [Test]
        public void TestParseRawRequest3()
        {
            var composer = new HttpRequestComposer();
            // semicolon is missing for Host heeader
            string rawRequest = "GET https://www.google.com/ HTTP/1.1\r\n" +
                                  "Host www.google.com\r\n" +
                                  "Connection: keep­alive\r\n" +
                                  "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8";

            bool result = composer.ParseRawRequest(rawRequest);
            Assert.AreEqual(result, false);
        }
        [Test]
        public void TestParseRawRequest4()
        {
            var composer = new HttpRequestComposer();

            // http version is missing
            string rawRequest = "GET https://www.google.com/\r\n" +
                                  "Host: www.google.com\r\n" +
                                  "Connection: keep­alive\r\n" +
                                  "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8";

            bool result = composer.ParseRawRequest(rawRequest);
            Assert.AreEqual(result, false);
        }

        [Test]
        public void TestParseRawRequest5()
        {
            var composer = new HttpRequestComposer();

            string rawRequest = "GET /index.php HTTP/1.1\r\n" +
                                  "Host: www.example.com.tr\r\n" +
                                  "Connection: keep­alive\r\n" +
                                  "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8";

            bool result = composer.ParseRawRequest(rawRequest);
            Assert.AreEqual(result, true);
        }

        [Test]
        public void TestValidateForm()
        {
            var composer = new HttpRequestComposer();

            bool result = composer.ValidateForm("index.html", "", "GET");

            Assert.AreEqual(result, false);
        }

        [Test]
        public void TestValidateForm2()
        {
            var composer = new HttpRequestComposer();

            bool result = composer.ValidateForm("http://example.com/index.html", "", "GET");

            Assert.AreEqual(result, true);
        }
        [Test]
        public void TestValidateForm3()
        {
            var composer = new HttpRequestComposer();

            bool result = composer.ValidateForm("", "", "");

            Assert.AreEqual(result, false);
        }
        [Test]
        public void TestValidateForm4()
        {
            var composer = new HttpRequestComposer();

            bool result = composer.ValidateForm("http://some invalid url/", "", "");

            Assert.AreEqual(result, false);
        }
        [Test]
        public void TestValidateForm5()
        {
            var composer = new HttpRequestComposer();

            bool result = composer.ValidateForm("index.html", "http://example.com/", "GET");

            Assert.AreEqual(result, true);
        }
        [Test]
        public async Task TestSendRequest()
        {
            var composer = new HttpRequestComposer();

            string rawRequest = "GET https://www.google.com/ HTTP/1.1\r\n" +
                                  "Host: www.google.com\r\n" +
                                  "Connection: keep­alive\r\n" +
                                  "Accept: text / html,application / xhtml + xml,application / xml;q = 0.9,image / webp,*/*;q=0.8\r\n" +
                                  "User-Agent:Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36\r\n" +
                                  "Accept-Encoding: gzip, deflate\r\n" +
                                  "Accept-Language: en­US,en;q=0.8";

            composer.ParseRawRequest(rawRequest);
            bool result = await composer.SendRequest();
            Assert.AreEqual(result, true);
        }

        [Test]
        public async Task TestSendRequest2()
        {
            var composer = new HttpRequestComposer();


            composer.InitializeHttpRequest("https://www.google.com/", "", "GET");
            bool result = await composer.SendRequest();
            Assert.AreEqual(result, true);
        }
        [Test]
        public async Task TestSendRequest3()
        {
            var composer = new HttpRequestComposer();


            composer.InitializeHttpRequest("", "www.google.com", "GET");
            bool result = await composer.SendRequest();
            Assert.AreEqual(result, true);
        }
    }
}
