using System.Net;

namespace ProxyRepeater.Server.Core
{
    public class ExClient
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }
    }
}