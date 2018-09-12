using System.Net;

namespace ProxyRepeater.Server.Core
{
    public class ExClient
    {
        private string _name;
        private IPAddress _ipAddress;

        public string Name { get => _name ?; set => _name = string.IsNullOrEmpty(value) ? throw new System.Exception($"Null value. Property: {nameof(Name)}") : value; }
        public IPAddress IpAddress { get => _ipAddress; set => _ipAddress = value ?? throw new System.Exception($"Null value. Property: {nameof(IpAddress)}"); }
        public int Port { get; set; }
    }
}