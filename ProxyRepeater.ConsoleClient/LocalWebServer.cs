using Nancy;
using Nancy.ModelBinding;

namespace ProxyRepeater.ConsoleClient
{
    internal class LocalWebServer : NancyModule
    {
        private const string OK = "OK";

        private class ServerMessageModel
        {
            public string Message { get; set; }
        }

        public LocalWebServer(IMessageHandler messageHandler)
        {
            Post["/"] = _ =>
             {
                 ServerMessageModel model = this.Bind<ServerMessageModel>();
                 messageHandler.HandleMessage(model.Message);
                 return OK;
             };
        }
    }
}
