using ProxyRepeater.Server.Core;
using System;
using System.Collections.Generic;

namespace ProxyRepeater.Server.Implementations
{
    public class MsgClientDispatcher : IExchanger
    {
        public ErrorNumber AddClient(ExClient client)
        {
            throw new NotImplementedException();
        }

        public void ClearClients()
        {
            throw new NotImplementedException();
        }

        public ErrorNumber DeleteClient(ExClient client)
        {
            throw new NotImplementedException();
        }

        public void DeliverMessage(IClientMsg msg)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExClient> GetClients()
        {
            throw new NotImplementedException();
        }
    }
}
