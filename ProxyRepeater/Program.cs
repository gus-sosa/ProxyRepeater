using CommandLine;
using LightInject;
using Nancy.Hosting.Self;
using ProxyRepeater.Server.Core;
using ProxyRepeater.Server.Implementations;
using System;

namespace ProxyRepeater.Server
{
    internal class Program
    {
        public static NancyHost WebApiHost { get; private set; }
        public static ServiceContainer Container { get; set; } = new ServiceContainer();

        public static IProxy ProxyServer { get; set; }

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
                   ProxyServer.Stop();
                   Container.Dispose();
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
            ProxyServer = Container.GetInstance<IProxy>();
            ProxyServer.Listen(proxyPort);
            Console.WriteLine($"Starting proxy at: http://localhost:{proxyPort}/");
        }

        private static void SetupDependencies() =>
            Container
                .Register(_ => new MsgClientDispatcher() , new PerContainerLifetime())
                .Register<IClientMsg , ClientMessage>()
                .Register<IExchanger>(factory => factory.GetInstance<MsgClientDispatcher>())
                .Register<IMsgDeliverer>(factory => factory.GetInstance<MsgClientDispatcher>())
                .Register<IProxy , TitaniumProxyServer>();
    }
}