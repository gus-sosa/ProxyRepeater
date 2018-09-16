using Nancy;
using Nancy.ModelBinding;

namespace ProxyRepeater.ConsoleClient
{
    public class LocalWebServer : NancyModule
    {
        private const string OK = "OK";
        private const string GOOD = "GOOD";

        private class ServerMessageModel
        {
            public string Message { get; set; }
        }

        public IMessageHandler MessageHandler { get; set; }

        public LocalWebServer(IMessageHandler messageHandler)
        {
            MessageHandler = messageHandler;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Get["/test"] = _ => GOOD;
            Post["/"] = _ => ProcessNewMessage();
        }

        private dynamic ProcessNewMessage()
        {
            ServerMessageModel model;
            try { model = this.Bind<ServerMessageModel>(); }
            catch (System.Exception e) { return Response.AsJson("Bad message" , HttpStatusCode.InternalServerError); }

            try { MessageHandler.HandleMessage(model.Message); }
            catch (System.Exception) { Response.AsJson("Internal server error" , HttpStatusCode.InternalServerError); }

            return OK;
        }
    }
}
