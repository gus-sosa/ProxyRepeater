using CommandLine;

namespace ProxyRepeater.Server
{
    internal class Program
    {
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

                   while (true) ;
               });

        private static void TurnOnWebApi(int webApiPort)
        {
            throw new System.NotImplementedException();
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