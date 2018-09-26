using ProxyRepeater.Server.Core;
using ProxyRepeater.Server.Implementations.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace ProxyRepeater.Server.Implementations
{
    public class TitaniumProxyServer : IProxy
    {
        protected const int DefaultProxyPort = 7546;

        public TitaniumProxyServer(IMsgDeliverer msgDeliverer , ProxyEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            _msgDeliverer = msgDeliverer ?? throw new ArgumentNullException(nameof(msgDeliverer));

            _proxyServer = new ProxyServer() { ForwardToUpstreamGateway = true };
            _proxyServer.CertificateManager.TrustRootCertificate();
            _proxyServer.AfterResponse += AfterResponseEvent;
            _proxyServer.AddEndPoint(endPoint);

            msgDeliverer.RestartDeliverProcess();
        }

        public TitaniumProxyServer(IMsgDeliverer deliverer) : this(deliverer , new ExplicitProxyEndPoint(IPAddress.Any , DefaultProxyPort , true)) { }

        private readonly IMsgDeliverer _msgDeliverer;
        private ProxyServer _proxyServer;

        public void Listen(int? port = null)
        {
            if (port.HasValue)
            {
                _proxyServer.ProxyEndPoints.Clear();
                _proxyServer.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Any , port.Value));
            }
            _proxyServer.Start();
        }

        public void Stop() => _proxyServer.Stop();

        private Task AfterResponseEvent(object sender , SessionEventArgs e)
            => Task.Factory.StartNew(() => _msgDeliverer.DeliverMessage(new HttpTitaniumSessionAdapter(e)));
    }
}
