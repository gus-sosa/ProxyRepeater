using CommandLine;
using LightInject;
using Nancy.Hosting.Self;
using ProxyRepeater.Server.Core;
using ProxyRepeater.Server.Implementations;
using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace ProxyRepeater.Server
{
    internal class Program
    {
        public static NancyHost WebApiHost { get; private set; }
        public static ServiceContainer Container { get; set; } = new ServiceContainer();

        public static ProxyServer ProxyServer { get; set; }

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
            ProxyServer = new ProxyServer();
            ProxyServer.CertificateManager.TrustRootCertificate();
            IMsgDeliverer deliverer = Container.GetInstance<IMsgDeliverer>();
            ProxyServer.AfterResponse += (object sender , SessionEventArgs e) => Task.Run(() => deliverer.DeliverMessage(new SessionAdapter(e)));
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any , proxyPort , true);
            ProxyServer.AddEndPoint(explicitEndPoint);
            Console.WriteLine($"Starting proxy at: http://localhost:{proxyPort}/");
            ProxyServer.Start();
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