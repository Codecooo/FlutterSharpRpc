using System.Threading.Tasks;

namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// Notifier interface for making notifications from server to flutter client
    /// </summary>
    public interface IRpcNotifier
    {
        /// <summary>
        /// Method for making notifications to flutter client through RPC asynchronous
        /// </summary>
        /// <param name="method">The name of the method to execute on the client side</param>
        /// <param name="parameter">Optional parameter to the method if they have one</param>
        /// <returns></returns>
        Task NotifyAsync(string method, object parameter = default);
    }
}