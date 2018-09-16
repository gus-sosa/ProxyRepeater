using Nancy;
using Nancy.ModelBinding;

namespace ProxyRepeater.ConsoleClient
{
    public class LocalWebServer : NancyModule
    {
        private const string OK = "OK";

        private class ServerMessageModel
        {
            public string Message { get; set; }
        }

        public IMessageHandler MessageHandler { get; set; } = new MessageHandler();

        public LocalWebServer()
        {
            Get["/test"] = _ => "GOOD";

            Post["/"] = _ =>
             {
                 ServerMessageModel model = this.Bind<ServerMessageModel>();
                 MessageHandler.HandleMessage(model.Message);
                 return OK;
             };
        }
    }
}
