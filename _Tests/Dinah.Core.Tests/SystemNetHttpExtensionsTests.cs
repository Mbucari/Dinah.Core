﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace SystemNetHttpExtensionsTests
{
    [TestClass]
    public class AddContent
    {
        [TestMethod]
        public void string_test()
        {
            var request = getEmptyMessage();

            var input = "my string";
            var content = new StringContent(input);

            request.AddContent(content);

            Assert.AreEqual(request.Content.Headers.ContentType.CharSet, "utf-8");
            Assert.AreEqual(request.Content.Headers.ContentType.MediaType, "text/plain");

            test_content(request, input);
        }

        [TestMethod]
        public void dictionary_test()
        {
            var request = getEmptyMessage();

            var dic = new Dictionary<string, string> { ["name1"] = "value 1", ["name2"] = "\"'&<>" };
            request.AddContent(dic);

            Assert.AreEqual(request.Content.Headers.ContentType.CharSet, null);
            Assert.AreEqual(request.Content.Headers.ContentType.MediaType, "application/x-www-form-urlencoded");

            test_content(request, "name1=value+1&name2=%22%27%26%3C%3E");
        }

        [TestMethod]
        public void json_test()
        {
            var request = getEmptyMessage();

            var jsonStr = "{\"name1\":\"value 1\"}";
            var json = JObject.Parse(jsonStr);
            request.AddContent(json);

            request.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
            request.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            test_content(request, JObject.Parse(jsonStr).ToString(Newtonsoft.Json.Formatting.Indented));
        }

        HttpRequestMessage getEmptyMessage()
        {
            var request = new HttpRequestMessage();
            Assert.AreEqual(request.Content, null);

            return request;
        }

        void test_content(HttpRequestMessage request, string expectedMessage)
        {
            var contentString = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual(expectedMessage, contentString);
        }
    }

    [TestClass]
    public class ParseCookie
    {
        [TestMethod]
        public void null_param_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => SystemNetHttpExtensions.ParseCookie(null));

        [TestMethod]
        public void test_cookie()
        {
            var cookie = SystemNetHttpExtensions.ParseCookie("session-id=139-1488065-0277455; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");
            cookie.Name.Should().Be("session-id");
            cookie.Value.Should().Be("139-1488065-0277455");
            cookie.Domain.Should().Be(".amazon.com");
            cookie.Path.Should().Be("/");
            cookie.Secure.Should().BeFalse();
            cookie.Expires.Should().Be(DateTime.Parse("Thu, 30-Jun-2039 19:07:14 GMT"));
        }
    }

	[TestClass]
	public class ReadAsJObjectAsync
	{
		[TestMethod]
		public async Task valid_FormUrlEncodedContent()
		{
			var message = new HttpResponseMessage
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["k1"] = "v1",
					["k2"] = "!@#$%^&*()<>-=_:'\"\\\n"
				}),
				StatusCode = System.Net.HttpStatusCode.OK
			};

			var str = await message.Content.ReadAsStringAsync();
			str.Should().Be("k1=v1&k2=%21%40%23%24%25%5E%26%2A%28%29%3C%3E-%3D_%3A%27%22%5C%0A");

			var jObj = await message.Content.ReadAsJObjectAsync();
			var json = jObj.ToString(Newtonsoft.Json.Formatting.Indented);
			var expected = @"
{
  ""k1"": ""v1"",
  ""k2"": ""!@#$%^&*()<>-=_:'\""\\\n""
}
		".Trim();

			json.Should().Be(expected);
		}

		[TestMethod]
		public async Task not_supported_HttpContent_type()
		{
			var message = new HttpResponseMessage
			{
				Content = new StreamContent(new MemoryStream()),
				StatusCode = System.Net.HttpStatusCode.OK
			};

			await Assert.ThrowsExceptionAsync<JsonReaderException>(() => message.Content.ReadAsJObjectAsync());
		}

		[TestMethod]
		public async Task invalid_json()
		{
			var message = new HttpResponseMessage
			{
				Content = new StringContent("{\"a\""),
				StatusCode = System.Net.HttpStatusCode.OK
			};
			await Assert.ThrowsExceptionAsync<JsonReaderException>(() => message.Content.ReadAsJObjectAsync());
		}

		[TestMethod]
		public async Task valid_json()
		{
			var message = new HttpResponseMessage
			{
				Content = new StringContent("{'a':1}"),
				StatusCode = System.Net.HttpStatusCode.OK
			};
			var jObj = await message.Content.ReadAsJObjectAsync();
			jObj.ToString(Newtonsoft.Json.Formatting.None).Should().Be("{\"a\":1}");
		}
	}

	[TestClass]
    public class DownloadFileAsync_ISealedHttpClient
    {
        [TestMethod]
        public async Task null_params_throw()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync((IHttpClientActions)null, "url", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<IHttpClientActions>().Object, null, "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<IHttpClientActions>().Object, "url", null));
        }

        [TestMethod]
        public async Task blank_params_throw()
        {
            var mock = new Mock<IHttpClientActions>().Object;
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "   ", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", "   "));
        }
    }

    [TestClass]
    public class DownloadFileAsync_HttpClient
    {
        [TestMethod]
        public async Task null_params_throw()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync((HttpClient)null, "url", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<HttpClient>().Object, null, "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<HttpClient>().Object, "url", null));
        }

        [TestMethod]
        public async Task blank_params_throw()
        {
            var mock = new Mock<HttpClient>().Object;
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "   ", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", "   "));
        }

		[TestMethod]
		public async Task rename_via_ContentDisposition()
		{
			var test_file_base64 = "dGVzdA==";
			var test_plaintext = "test";

			var expectedFilename = "file.aax";

			try
			{
				var response = new HttpResponseMessage
				{
					Content = new ByteArrayContent(Convert.FromBase64String(test_file_base64))
				};
				response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "foo_ep6.aax" };

				var client = new HttpClient(HttpMock.GetHandler(response));

				var finalFile = await client.DownloadFileAsync("http://t.co.uk/downloadme?a=1", "file.xyz");

				await Task.Delay(100);
				finalFile.Should().Be(expectedFilename);
				File.Exists(expectedFilename).Should().BeTrue();
				File.ReadAllText(expectedFilename).Should().Be(test_plaintext);
			}
			finally
			{
				File.Delete(expectedFilename);
			}
		}
	}
}
