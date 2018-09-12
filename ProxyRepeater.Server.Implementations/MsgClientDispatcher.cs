using ProxyRepeater.Server.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProxyRepeater.Server.Implementations
{
    public class MsgClientDispatcher : IExchanger
    {
        public ConcurrentDictionary<string , ExClient> Clients { get; set; }

        public ErrorNumber AddClient(ExClient client) => Clients.TryAdd(client.Name , client) ? ErrorNumber.NoError : ErrorNumber.NoError;

        public void ClearClients() => Clients.Clear();

        public ErrorNumber DeleteClient(ExClient client) => Clients.TryRemove(client.Name , out _) ? ErrorNumber.NoError : ErrorNumber.ClientDoesNotExist;

        public IEnumerable<ExClient> GetClients() => Clients.Values;

        public void DeliverMessage(IClientMsg msg)
        {
            throw new NotImplementedException();
        }
    }
}
