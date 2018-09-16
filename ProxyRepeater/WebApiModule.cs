using Nancy;
using ProxyRepeater.Server.Core;
using System.Net;

namespace ProxyRepeater.Server
{
    public class WebApiModule : NancyModule
    {
        private readonly IExchanger exchanger;

        private const string OK = "OK";
        private const string FAILED = "";

        public WebApiModule(IExchanger exchanger)
        {
            this.exchanger = exchanger ?? throw new System.ArgumentNullException(nameof(exchanger));
            Get["/test"] = _ => "GOOD - SERVER";
            Get["/"] = _ => exchanger.GetClients();
            Post["/{clientName}/{port:int}"] = parameters =>
            {
                IPAddress.TryParse(Context.Request.UserHostAddress , out IPAddress ip);
                return AddClient(parameters.clientName , ip , parameters.port);
            };
            Delete["/{clientName}"] = parameters => RemoveClient(parameters.clientName);
            Get["/Ping/{clientName}"] = parameters => PingClient(parameters.clientName);
        }

        private dynamic PingClient(string clientName) => exchanger.GetClient(clientName) == null ? "Ok" : $"{FAILED}: That client does not exist";

        private dynamic RemoveClient(string clientName)
        {
            ExClient client = exchanger.GetClient(clientName);
            if (client == null)
                return $"{FAILED}: That client does not exist";

            exchanger.DeleteClient(client);
            return OK;
        }

        private dynamic AddClient(string clientName , IPAddress ip , int port)
        {
            var client = new ExClient() { Name = clientName , IpAddress = ip , Port = port };
            ErrorNumber error = exchanger.AddClient(client);
            return error == ErrorNumber.NoError ? OK : $"Problem with the request: ErrorNumber: {error}";
        }
    }
}
