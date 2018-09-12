using ProxyRepeater.Server.Core;
using System.Collections.Generic;

namespace ProxyRepeater.Server.Implementations
{
    public class MessageExchanger : IExchanger
    {
        public ErrorNumber AddClient(ExClient client)
        {
            throw new System.NotImplementedException();
        }

        public ErrorNumber DeleteClient(ExClient client)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ExClient> GetClients()
        {
            throw new System.NotImplementedException();
        }

        public void ClearClients()
        {
            throw new System.NotImplementedException();
        }

        public void DeliverMessage(IClientMsg msg)
        {
            throw new System.NotImplementedException();
        }
    }
}