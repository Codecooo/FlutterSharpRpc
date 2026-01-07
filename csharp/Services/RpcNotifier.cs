using System;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// Notifier base class for making notifications from server to flutter client
    /// </summary>
    public abstract class RpcNotifier : IRpcNotifier
    {
        /// <summary>
        /// The JsonRpc instance used to make notifications
        /// </summary>
        public JsonRpc JsonRpc { get; private set; }

        /// <summary>
        /// Attach notifier to the class so they can start notify others
        /// </summary>
        /// <param name="notifier"></param>
        internal void AttachNotifier(JsonRpc notifier)
        {
            this.JsonRpc = notifier;
        }

        [Obsolete("Use NotifyAsync directly from JsonRpc property in this class instead.")]
        public Task NotifyAsync(string method, object payload = null)
        {
            return payload is null
                ? JsonRpc.NotifyAsync(method)
                : JsonRpc.NotifyAsync(method, payload);
        }
    }
}