using ProxyRepeater.Server.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Titanium.Web.Proxy.EventArguments;

namespace ProxyRepeater.Server.Implementations.Models
{
    public enum Method
    {
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
        CONNECT,
        OPTIONS,
        TRACE,
        PATCH,
        UNKNOWN
    }

    public class HttpMsg : IClientMsg
    {
        private const int NumDashToDelimitSessions = 20;

        public Method Method { get; set; }

        public string HttpVersion { get; set; }

        public string Url { get; set; }

        public List<(string name, string value)> RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public int ResponseStatusCode { get; set; }

        internal static Method GetMethod(string method) => Enum.TryParse<Method>(method , out Method parsedMethod) ? parsedMethod : Method.UNKNOWN;

        internal static IEnumerable<Method> RequestsWithBody = new[] { Method.POST , Method.PUT , Method.PATCH };

        internal static string GetRequestBody(SessionEventArgs session)
        {
            Method method = GetMethod(session.WebSession.Request.Method);
            return RequestsWithBody.Contains(method) && session.WebSession.Request.ContentLength > 0 ? session.GetRequestBodyAsString().Result : string.Empty;
        }

        internal static string GetResponseBody(SessionEventArgs session)
        {
            Method method = GetMethod(session.WebSession.Request.Method);
            return RequestsWithBody.Contains(method) && session.WebSession.Response.ContentLength > 0 ? session.GetResponseBodyAsString().Result : string.Empty;
        }

        public string ResponseStatusDescription { get; set; }

        public List<(string name, string value)> ResponseHeaders { get; set; }

        public string ResponseBody { get; set; }

        public string GetMessage() => GetSessionString();

        public string GetSessionString() => $"{GetSessionRequestString()}\n{new string('-' , NumDashToDelimitSessions)}\n{GetSessionResponseString()}";

        public string GetSessionRequestString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{GetVerb()} {Url} {HttpVersion}");
            AppendHeadersString(RequestHeaders , stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.Append(RequestBody);
            return stringBuilder.ToString();
        }

        private void AppendHeadersString(IEnumerable<(string name, string value)> headers , StringBuilder stringBuilder)
        {
            foreach ((string Name, string Value) httpHeader in headers)
                stringBuilder.AppendLine($"{httpHeader.Name}: {httpHeader.Value}");
        }

        public string GetVerb() => Method.ToString().ToUpper();

        public string GetSessionResponseString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{HttpVersion} {ResponseStatusCode} {ResponseStatusDescription}");
            AppendHeadersString(ResponseHeaders , stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.Append(ResponseBody);
            return stringBuilder.ToString();
        }
    }
}
