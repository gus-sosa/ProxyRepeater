using System.Collections.Generic;

namespace ProxyRepeater.Server.Core
{
    public interface IExchanger
    {
        ErrorNumber AddClient(ExClient client);
        ErrorNumber DeleteClient(ExClient client);
        IEnumerable<ExClient> GetClients();
        void ClearClients();
        void DeliverMessage(IClientMsg msg);
    }
}