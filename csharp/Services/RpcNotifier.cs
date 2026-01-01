using System.Threading.Tasks;
using StreamJsonRpc;

namespace FlutterSharpRpc.Services
{
    public class RpcNotifier : IRpcNotifier
    {
        private readonly JsonRpc _jsonRpc;

        public RpcNotifier(JsonRpc jsonRpc)
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