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
            Func<JsonRpc, TServer, Task> registerMethods = default,
            CancellationToken cancellationToken = default)
            where TServer : class
        {
            await using var stdin = Console.OpenStandardInput();
            await using var stdout = Console.OpenStandardOutput();

            var duplex = FullDuplexStream.Splice(stdin, stdout);

            formatter.JsonSerializerOptions.PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase;

            formatter.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

            using var handler = new HeaderDelimitedMessageHandler(
                duplex,
                duplex,
                formatter);

            using var rpc = new JsonRpc(handler);

            if (server is IRpcNotifierAware rpcNotifierAware)
            {
                rpcNotifierAware.AttachNotifier(new RpcNotifier(rpc));
            }

            // If we dont set registerMethods it means we are using normal mode instead of aot
            if (registerMethods == default)
            {
                rpc.AddLocalRpcTarget(server);
                RpcLog.Info($"RPC target registered: {typeof(TServer).FullName}");
            }
            else
            {
                await registerMethods(rpc, server);
                RpcLog.Info($"RPC target registered: {typeof(TServer).FullName}");
            }

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