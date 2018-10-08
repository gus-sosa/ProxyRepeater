using System.Threading.Tasks;

namespace ProxyRepeater.Server.Implementations.Models
{
    public interface ISessionReader<T>
    {
        T ReadRequestHeaders();

        T ReadResponseHeaders();
        Task<T> ReadRequestBody();
        Task<T> ReadResponseBody();
    }
}