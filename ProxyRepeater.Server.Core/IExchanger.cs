using System.Collections.Generic;

namespace ProxyRepeater.Server.Core
{
    public interface IExchanger : IMsgDeliverer
    {
        ErrorNumber AddClient(ExClient client);
        ErrorNumber DeleteClient(ExClient client);
        IEnumerable<ExClient> GetClients();
        ExClient GetClient(string name);
        void ClearClients();
    }
}