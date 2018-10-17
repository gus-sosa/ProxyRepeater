using ProxyRepeater.Server.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace ProxyRepeater.Server.Implementations.Models
{
    public class HttpTitaniumSessionAdapter : ISessionReader<HttpTitaniumSessionAdapter>, IClientMsg
    {
        public HttpTitaniumSessionAdapter(SessionEventArgs e) => TitaniumSession = e ?? throw new ArgumentNullException(nameof(e));

        private SessionEventArgs TitaniumSession { get; set; }

        private string _requestHeaders = string.Empty;
        private string _requestBody = string.Empty;
        private string _responseHeaders = string.Empty;
        private string _responseBody = string.Empty;

        public string GetMessage() => $"{_requestHeaders}\n{_requestBody}{_responseHeaders}\n{_responseBody}";

        private List<(string name, string value)> ConvertHeaders(HeaderCollection headers)
        {
            var list = new List<(string name, string value)>();
            foreach (HttpHeader header in headers)
                list.Add(new ValueTuple<string , string>(header.Name , header.Value));
            return list;
        }

        private string HeadersToString(List<(string name, string value)> list)
        {
            var strBuilder = new StringBuilder();
            foreach ((string name, string value) item in list)
                strBuilder.Append($"{item.name}: {item.value}");
            return strBuilder.ToString();
        }

        public HttpTitaniumSessionAdapter ReadRequestHeaders()
        {
            _requestHeaders = HeadersToString(ConvertHeaders(TitaniumSession.WebSession.Request.Headers));
            return this;
        }

        public HttpTitaniumSessionAdapter ReadResponseHeaders()
        {
            _responseHeaders = HeadersToString(ConvertHeaders(TitaniumSession.WebSession.Response.Headers));
            return this;
        }

        public async Task<HttpTitaniumSessionAdapter> ReadRequestBody()
        {
            _requestBody = await TitaniumSession.GetRequestBodyAsString();
            return this;
        }

        public async Task<HttpTitaniumSessionAdapter> ReadResponseBody()
        {
            _responseBody = await TitaniumSession.GetResponseBodyAsString();
            TitaniumSession.SetResponseBodyString(_responseBody);
            return this;
        }

        internal HttpTitaniumSessionAdapter UpdateSession(SessionEventArgs e)
        {
            TitaniumSession = e;
            return this;
        }
    }
}
