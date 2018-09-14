using ProxyRepeater.Server.Core;
using System.Text;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;

namespace ProxyRepeater.Server
{
    internal class SessionAdapter : IClientMsg
    {
        private readonly SessionEventArgs e;
        private const int NumDashes = 15;

        public SessionAdapter(SessionEventArgs e) => this.e = e;

        public string GetMessage()
        {
            var stringBuilder = new StringBuilder();
            AddRequest(stringBuilder);
            stringBuilder.AppendLine(new string('-' , NumDashes));
            AddResponse(stringBuilder);
            return stringBuilder.ToString();
        }

        private void AddResponse(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"{GetHttpVersion(e.WebSession.Response)} {GetStatusCodeNumber()} {GetStatusCodeDescription()}");
            stringBuilder.AppendLine(GetHeadersString(e.WebSession.Response.Headers));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(GetBody(e.WebSession.Response));
        }

        private void AddRequest(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"{GetVerb()} {GetUrl()} {GetHttpVersion(e.WebSession.Request)}");
            stringBuilder.AppendLine(GetHeadersString(e.WebSession.Request.Headers));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(GetBody(e.WebSession.Request));
        }

        private string GetBody(RequestResponseBase request) => request.BodyString;

        private string GetHeadersString(HeaderCollection headers)
        {
            var stringBuilder = new StringBuilder();
            foreach (Titanium.Web.Proxy.Models.HttpHeader item in headers)
                stringBuilder.AppendLine($"{item.Name}: {item.Value}");
            return stringBuilder.ToString();
        }

        private string GetVerb() => e.WebSession.Request.Method;

        private string GetUrl() => e.WebSession.Request.Url;

        private string GetHttpVersion(RequestResponseBase e) => e.HttpVersion.ToString();

        private string GetStatusCodeDescription() => e.WebSession.Response.StatusDescription;

        private string GetStatusCodeNumber() => e.WebSession.Response.StatusCode.ToString();
    }
}