using CommandLine;
using Nancy.Hosting.Self;
using System;

namespace ProxyRepeater.Server
{
    internal class Program
    {
        public static NancyHost WebApiHost { get; private set; }

        private class CommandLineArguments
        {
            [Option(HelpText = "Set port to listen from services" , Default = 8888)]
            public int Port { get; set; }

            [Option(HelpText = "Set port to receive commands from clients" , Default = 8080)]
            public int WebApiPort { get; set; }
        }

        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<CommandLineArguments>(args)
               .WithParsed(o =>
               {
                   SetupDependencies();
                   TurnOnProxy(o.Port);
                   TurnOnWebApi(o.WebApiPort);

                   Console.ReadLine();

                   WebApiHost.Stop();
               });

        private static void TurnOnWebApi(int webApiPort)
        {
            var uri = new Uri($"http://localhost:{webApiPort}");
            WebApiHost = new NancyHost(uri);
            WebApiHost.Start();
            Console.WriteLine($"Web api running on: {uri.AbsoluteUri}");
        }

        private static void TurnOnProxy(int proxyPort)
        {
            throw new System.NotImplementedException();
        }

        private static void SetupDependencies()
        {
            throw new System.NotImplementedException();
        }
    }
}