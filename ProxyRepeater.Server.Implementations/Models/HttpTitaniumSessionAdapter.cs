using ProxyRepeater.Server.Core;
using System;
using System.Collections.Generic;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace ProxyRepeater.Server.Implementations.Models
{
    public class HttpTitaniumSessionAdapter : IClientMsg
    {
        public HttpTitaniumSessionAdapter(SessionEventArgs e) => TitaniumSession = e ?? throw new ArgumentNullException(nameof(e));

        private SessionEventArgs TitaniumSession { get; }

        public string GetMessage()
            => new HttpMsg()
            {
                HttpVersion = TitaniumSession.WebSession.Request.HttpVersion.ToString() ,
                Method = HttpMsg.GetMethod(TitaniumSession.WebSession.Request.Method) ,
                RequestBody = TitaniumSession.WebSession.Request.BodyString ,
                RequestHeaders = ConvertHeaders(TitaniumSession.WebSession.Request.Headers) ,
                ResponseBody = TitaniumSession.WebSession.Response.BodyString ,
                ResponseHeaders = ConvertHeaders(TitaniumSession.WebSession.Response.Headers) ,
                ResponseStatusCode = TitaniumSession.WebSession.Response.StatusCode ,
                ResponseStatusDescription = TitaniumSession.WebSession.Response.StatusDescription ,
                Url = TitaniumSession.WebSession.Request.Url
            }.GetMessage();

        private List<(string name, string value)> ConvertHeaders(HeaderCollection headers)
        {
            var list = new List<(string name, string value)>();
            foreach (HttpHeader header in headers)
                list.Add(new ValueTuple<string , string>(header.Name , header.Value));
            return list;
        }
    }
}
