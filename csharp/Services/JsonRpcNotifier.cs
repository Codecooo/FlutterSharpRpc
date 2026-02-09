using System.Threading.Tasks;
using StreamJsonRpc;

namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// Notifier base class for making notifications from server to flutter client
    /// Only used for cases where the server class does not inherit from RpcNotifier
    /// </summary>
    internal class JsonRpcNotifier : IRpcNotifier
    {
        private JsonRpc _jsonRpc;

        public JsonRpcNotifier(JsonRpc jsonRpc)
        {
            _jsonRpc = jsonRpc;
        }

        public Task NotifyAsync(string method, object payload = null)
        {
            return payload is null
                ? _jsonRpc.NotifyAsync(method)
                : _jsonRpc.NotifyAsync(method, payload);
        }
    }
}