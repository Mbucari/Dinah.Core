﻿using System;
using System.Net;
using System.Net.Http.Headers;

namespace Dinah.Core.Net.Http
{
	public interface IHttpClient : IHttpClientActions
	{
		CookieContainer CookieJar { get; }
		HttpRequestHeaders DefaultRequestHeaders { get; }
		Uri BaseAddress { get; set; }
		long MaxResponseContentBufferSize { get; set; }
		TimeSpan Timeout { get; set; }
	}
}
