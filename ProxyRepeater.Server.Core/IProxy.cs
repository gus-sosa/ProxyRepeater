namespace ProxyRepeater.Server.Core
{
    public interface IProxy
    {
        void Listen(int? port = null);
        void Stop();
    }
}