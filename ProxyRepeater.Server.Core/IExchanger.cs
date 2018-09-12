using System.Collections.Generic;

namespace ProxyRepeater.Server.Core
{
    public interface IMsgDeliverer : IMsgDeliverer
    {
        ErrorNumber AddClient(ExClient client);
        ErrorNumber DeleteClient(ExClient client);
        IEnumerable<ExClient> GetClients();
        void ClearClients();
    }
}