using CommandLine;
using System;
using System.Text.RegularExpressions;

namespace ProxyRepeater.ConsoleClient
{
    internal class Program
    {
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
        }

        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed(options =>
                {
                    RunLocalWebServer(options.PortToListen);
                    ConnectWithProxyRepeater(options.ServerIp , options.PortToListen);
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
