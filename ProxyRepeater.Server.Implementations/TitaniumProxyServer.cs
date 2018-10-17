using ProxyRepeater.Server.Core;
using ProxyRepeater.Server.Implementations.Models;
using System;
using System.Collections.Concurrent;
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
        protected ConcurrentDictionary<Guid , HttpTitaniumSessionAdapter> sessionRecords = new ConcurrentDictionary<Guid , HttpTitaniumSessionAdapter>();

        public TitaniumProxyServer(IMsgDeliverer msgDeliverer , ProxyEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            _msgDeliverer = msgDeliverer ?? throw new ArgumentNullException(nameof(msgDeliverer));

            _proxyServer = new ProxyServer() { ForwardToUpstreamGateway = true };
            _proxyServer.CertificateManager.TrustRootCertificate();
            _proxyServer.BeforeResponse += BeforeResponseEvent;
            _proxyServer.BeforeRequest += BeforeRequestEvent;
            _proxyServer.AddEndPoint(endPoint);

            msgDeliverer.RestartDeliverProcess();
        }

        public TitaniumProxyServer(IMsgDeliverer deliverer) : this(deliverer , new ExplicitProxyEndPoint(IPAddress.Any , DefaultProxyPort , true)) { }

        private readonly IMsgDeliverer _msgDeliverer;
        private ProxyServer _proxyServer;

        private async Task BeforeRequestEvent(object sender , SessionEventArgs e)
        {
            var id = Guid.NewGuid();
            e.UserData = id;
            sessionRecords[id] = await new HttpTitaniumSessionAdapter(e)
                                                                .ReadRequestHeaders()
                                                                .ReadRequestBody();
        }

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

        private async Task BeforeResponseEvent(object sender , SessionEventArgs e)
        {
            var key = (Guid)e.UserData;
            if (sessionRecords.TryRemove(key , out HttpTitaniumSessionAdapter sessionAdapter))
            {
                await sessionAdapter
                        .UpdateSession(e)
                        .ReadResponseHeaders()
                        .ReadResponseBody();
                _msgDeliverer.DeliverMessage(sessionAdapter);
            }
        }
    }
}
