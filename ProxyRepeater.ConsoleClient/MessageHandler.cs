using System;
using System.IO;

namespace ProxyRepeater.ConsoleClient
{
    internal class MessageHandler : IMessageHandler
    {
        public void HandleMessage(string message)
        {
            using (StreamWriter file = File.CreateText($".\\{Guid.NewGuid()}.msg"))
                file.WriteLine(message);
        }
    }
}