using Nancy;
using ProxyRepeater.Server.Core;
using System.Net;

namespace ProxyRepeater.Server
{
    public class WebApiModule : NancyModule
    {
        private class ErrorMessage
        {
            public string ErrorDescription { get; set; }
            public int ErrorNumber { get; set; }

            public static ErrorMessage NewErrorMessage(string errorDescription , int errorNumber) => new ErrorMessage() { ErrorDescription = errorDescription , ErrorNumber = errorNumber };
        }

        private readonly IExchanger exchanger;

        private const string OK = "OK";
        private const string FAILED = "";

        public WebApiModule(IExchanger exchanger)
        {
            this.exchanger = exchanger ?? throw new System.ArgumentNullException(nameof(exchanger));
            Get["/test"] = _ => "GOOD - SERVER";
            Get["/"] = _ => Response.AsJson(exchanger.GetClients());
            Post["/{clientName}/{port:int}"] = parameters =>
            {
                IPAddress.TryParse(Context.Request.UserHostAddress , out IPAddress ip);
                return AddClient(parameters.clientName , ip , parameters.port);
            };
            Delete["/{clientName}"] = parameters => RemoveClient(parameters.clientName);
            Get["/Ping/{clientName}"] = parameters => PingClient(parameters.clientName);
        }

        private dynamic PingClient(string clientName) => exchanger.GetClient(clientName) != null ? "Ok" : Response.AsJson(ErrorMessage.NewErrorMessage($"{FAILED}: That client does not exist" , (int)ErrorNumber.ClientDoesNotExist) , Nancy.HttpStatusCode.InternalServerError);

        private dynamic RemoveClient(string clientName)
        {
            ExClient client = exchanger.GetClient(clientName);
            if (client == null)
                return Response.AsJson(ErrorMessage.NewErrorMessage($"{FAILED}: That client does not exist" , (int)ErrorNumber.ClientDoesNotExist) , Nancy.HttpStatusCode.InternalServerError);

            exchanger.DeleteClient(client);
            return OK;
        }

        private dynamic AddClient(string clientName , IPAddress ip , int port)
        {
            var client = new ExClient() { Name = clientName , IpAddress = ip , Port = port };
            ErrorNumber error = exchanger.AddClient(client);
            return error == ErrorNumber.NoError ? OK : Response.AsJson(ErrorMessage.NewErrorMessage($"Problem with the request: ErrorNumber: {error}" , (int)ErrorNumber.ExistingClient));
        }
    }
}
