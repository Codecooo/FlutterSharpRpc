using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace FlutterSharpRpc.Services
{
    internal class ServerStartup
    {
        internal static async Task StartServer<TServer>(
            TServer server,
            SystemTextJsonFormatter formatter,
            CancellationToken cancellationToken = default)
            where TServer : class
        {
            using (var stdin = Console.OpenStandardInput())
            using (var stdout = Console.OpenStandardOutput())
            using (var duplex = FullDuplexStream.Splice(stdin, stdout))
            {
                formatter.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;

                formatter.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

                using (var handler = new HeaderDelimitedMessageHandler(
                    duplex,
                    duplex,
                    formatter))
                using (var rpc = new JsonRpc(handler))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (server is RpcNotifier rpcNotifier)
                    {
                        rpcNotifier.AttachNotifier(rpc);
                    }
                    else if (server is IRpcNotifierAware notifierAware)
                    {
                        notifierAware.AttachNotifier(new JsonRpcNotifier(rpc));
                    }
#pragma warning restore CS0618 // Type or member is obsolete

                    rpc.AddLocalRpcTarget(server);
                    RpcLog.Info($"RPC target registered: {typeof(TServer).FullName}");

                    rpc.Disconnected += (_, e) => RpcLog.Warn($"RPC disconnected: {e.Reason}");

                    rpc.StartListening();
                    RpcLog.Info("csharp-json-rpc server started. Waiting for requests...");

                    using (cancellationToken.Register(() =>
                    {
                        RpcLog.Warn("Cancellation requested, disposing RPC");
                        rpc.Dispose();
                    }))
                    {
                        await rpc.Completion.ConfigureAwait(false);
                    }
                }
            }
        }
    }
}