using CommandLine;
using Flurl.Http;
using Nancy.Hosting.Self;
using Polly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProxyRepeater.ConsoleClient
{
    internal class Program
    {
        private const double _timeToWaitBeforeRetry = 3000;
        private const int _numTimesToRetry = 3;

        public static NancyHost Host { get; private set; }

        private class ConsoleOptions
        {
            private string _serverIp;
            private readonly Regex ipRegex = new Regex("\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b");

            [Option(shortName: 's' , longName: "server-ip" , HelpText = "Proxy repeater ip" , Required = true)]
            public string ServerIp
            {
                get => _serverIp;
                set
                {
                    if (!ipRegex.IsMatch(value))
                        throw new ArgumentException("The server ip provided is not valid");
                    _serverIp = value;
                }
            }

            [Option(shortName: 'p' , longName: "server-port" , HelpText = "Port of the proxy repeater server to reach out" , Required = true)]
            public int ServerPort { get; set; }

            [Option(shortName: 'l' , longName: "listen-port" , HelpText = "Local port to listen for messages from the server" , Required = true)]
            public int PortToListen { get; set; }

            [Option(shortName: 'c' , longName: "client-name" , HelpText = "Name or unique identifier of this client in the server" , Required = true)]
            public string ClientName { get; set; }
        }

        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed(options =>
                {
                    RunLocalWebServer(options.PortToListen);
                    ConnectWithProxyRepeater(options.ServerIp , options.ServerPort , options.ClientName , options.PortToListen);
                    Console.WriteLine("Press enter to end the process");
                    Console.WriteLine();
                    Host.Stop();
                });

        private static async void ConnectWithProxyRepeater(string serverIp , int serverPort , string clientName , int portToListen)
        {
            PolicyResult<HttpResponseMessage> httpResponseMsg = await Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(_timeToWaitBeforeRetry) , _numTimesToRetry) , (exception , retryCount , context) =>
                {
                    //TODO-LOG:  Log error+count
                })
                .ExecuteAndCaptureAsync(async () => await Task.Factory.StartNew(() =>
                     $"https://{serverIp}:{serverPort}/".PostJsonAsync(new { clientName , port = portToListen }).Result
                 ));

            Console.WriteLine(httpResponseMsg.Outcome == OutcomeType.Failure ? "Problem connecting to Proxy Repeater server" : $"Connected to Proxy Repeater at: {serverIp}");
        }

        private static IPAddress GetIp() => Dns.GetHostByName(Dns.GetHostName()).AddressList[0];

        private static void RunLocalWebServer(int portToListen)
        {
            Host = new NancyHost(new Uri($"http://localhost:{portToListen}"));
            Host.Start();
            Console.WriteLine($"Running local server on http://localhost:{portToListen}");
        }
    }
}
