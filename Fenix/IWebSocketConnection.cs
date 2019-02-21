using System.Threading.Tasks;

namespace Fenix
{
    public interface IWebSocketConnection
    {
        Task ConnectAsync(Options options);
    }
}