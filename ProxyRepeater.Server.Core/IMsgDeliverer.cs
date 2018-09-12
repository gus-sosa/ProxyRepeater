namespace ProxyRepeater.Server.Core
{
    public interface IMsgDeliverer
    {
        void DeliverMessage(IClientMsg msg);
    }
}