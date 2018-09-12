using ProxyRepeater.Server.Core;
using System;

namespace ProxyRepeater.Server.Implementations
{
    public class TitaniumProxyServer : IProxy
    {
        public TitaniumProxyServer(IExchanger exchanger)
            => Exchanger = exchanger ?? throw new ArgumentNullException(nameof(exchanger));

        private IExchanger Exchanger { get; set; }
        public void Listen(int port)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
