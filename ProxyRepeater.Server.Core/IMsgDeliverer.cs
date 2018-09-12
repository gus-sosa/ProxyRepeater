namespace ProxyRepeater.Server.Core
{
    public interface IMsgDeliverer
    {
        void DeliverMessage(IClientMsg msg);

        void StartDeliverProcess();
        void StopDeliverProcess();

        void RestartDeliverProcess();
    }
}