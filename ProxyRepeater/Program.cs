using CommandLine;

namespace ProxyRepeater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineArguments>(args)
                .WithParsed(o =>
                {
                });
        }

        private class CommandLineArguments
        {
            [Option(HelpText = "Set port to listen from services")]
            public int Port { get; set; }

            [Option(HelpText = "Set port to receive commands from clients")]
            public int WebApiPort { get; set; }
        }
    }
}
