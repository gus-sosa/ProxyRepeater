namespace ProxyRepeater.Server.Core
{
    public interface IProxy
    {
        IExchanger Exchanger { get; set; }
        void Listen(int port);
        void Stop();
    }
}