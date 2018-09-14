using CommandLine;
using System;
using System.Text.RegularExpressions;

namespace ProxyRepeater.ConsoleClient
{
    internal class Program
    {
        private class ConsoleOptions
        {
            private string _serverName;
            private readonly Regex ipRegex = new Regex("sadf");
            private readonly Regex serverNameRegex = new Regex("asdf");

            [Option(shortName: 's' , longName: "server-name" , HelpText = "Ip or address to the proxy repeater server" , Required = true)]
            public string ServerName
            {
                get => _serverName;
                set
                {
                    if (!ipRegex.IsMatch(value) && !serverNameRegex.IsMatch(value))
                        throw new ArgumentException("The server name provided is not valid");
                    _serverName = value;
                }
            }

            [Option(shortName: 'p' , longName: "server-port" , HelpText = "Port of the proxy repeater server to reach out" , Required = true)]
            public int ServerPort { get; set; }

            [Option(shortName: 'l' , longName: "listen-port" , HelpText = "Local port to listen for messages from the server" , Required = true)]
            public int PortToListen { get; set; }
        }

        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed(options =>
                {
                    RunLocalWebServer(options.PortToListen);
                    ConnectWithProxyRepeater(options.ServerName , options.PortToListen);
                });

        private static void ConnectWithProxyRepeater(string serverName , int portToListen)
        {
            throw new NotImplementedException();
        }

        private static void RunLocalWebServer(int portToListen)
        {
            throw new NotImplementedException();
        }
    }
}
