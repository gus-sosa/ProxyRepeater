using ProxyRepeater.Server.Core;

namespace ProxyRepeater.Server.Implementations
{
    public class ClientMessage : IClientMsg
    {
        public string Content { get; set; }

        public string GetMessage() => Content;
    }
}