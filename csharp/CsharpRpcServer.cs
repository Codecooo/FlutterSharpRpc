using System;
using System.Threading;
using System.Threading.Tasks;
using FlutterSharpRpc.Services;
using StreamJsonRpc;

namespace FlutterSharpRpc
{
    /// <summary>
    /// Manage csharp-json-rpc server
    /// </summary>
    public static class CsharpRpcServer
    {
        /// <summary>
        /// Start a csharp-json-rpc server and starts listening to incoming messages.
        /// </summary>
        /// <typeparam name="TServer">Type of class that contains the PRC methods</typeparam>
        /// <param name="server">Instance of class that contains the PRC methods</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel RPC</param>
        /// <returns>Task that resolve when the process stops</returns>
        public static async Task StartAsync<TServer>(
            TServer server,
            CancellationToken cancellationToken = default)
            where TServer : class
        {
            RpcLog.Info("Starting csharp-json-rpc server....");
            RpcLog.Info($"Runtime: {Environment.Version}");
            RpcLog.Info($"OS: {Environment.OSVersion}");
            #if NET6_0_OR_GREATER
                RpcLog.Info($"ProcessId: {Environment.ProcessId}");
            #endif

            try
            {
                var formatter = new SystemTextJsonFormatter();
                await ServerStartup.StartServer(server, formatter, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                RpcLog.Error("Fatal RPC server failure", ex);
                throw;
            }
        }
    }
}
