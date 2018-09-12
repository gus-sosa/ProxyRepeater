using ProxyRepeater.Server.Core;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

namespace ProxyRepeater.Server.Implementations
{
    public class TitaniumProxyServer : IProxy
    {
        protected const int DefaultProxyPort = 7546;

        public TitaniumProxyServer(IMsgDeliverer msgDeliverer , ProxyEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            Exchanger = msgDeliverer ?? throw new ArgumentNullException(nameof(msgDeliverer));

            _proxyServer = new ProxyServer();
            _proxyServer.CertificateManager.TrustRootCertificate();
            _proxyServer.AfterResponse += AfterResponseEvent;
            _proxyServer.AddEndPoint(endPoint);
        }

        public TitaniumProxyServer(IMsgDeliverer deliverer) : this(deliverer , new ExplicitProxyEndPoint(IPAddress.Any , DefaultProxyPort , true)) { }

        private IMsgDeliverer Exchanger { get; set; }
        private ProxyServer _proxyServer;

        public void Listen(int port) => _proxyServer.Start();

        public void Stop() => _proxyServer.Stop();

        private Task AfterResponseEvent(object sender , Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
