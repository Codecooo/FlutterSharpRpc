using System;
using System.Text.Json.Serialization.Metadata;
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

        /// <summary>
        /// Start a csharp-json-rpc server with explicit methods and types to allow
        /// it compatible for AOT environment.
        /// </summary>
        /// <typeparam name="TServer">Type of class that contains the PRC methods</typeparam>
        /// <param name="server">Instance of class that contains the PRC methods</param>
        /// <param name="jsonTypeInfo">Json type resolver from JsonSerializerContext so we can prevent reflection</param>
        /// <param name="registerMethods">A delegate to register methods explicitly without dynamic reflection</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel RPC</param>
        /// <returns></returns>
        public static async Task StartWithExplicitAsync<TServer>(
            TServer server,
            IJsonTypeInfoResolver jsonTypeInfo,
            Func<JsonRpc, TServer, Task> registerMethods,
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
                // Combine the type info from out base json context with consumer provided one
                formatter.JsonSerializerOptions.TypeInfoResolver =
                    JsonTypeInfoResolver.Combine(
                        jsonTypeInfo,
                        RpcJsonContext.Default
                );

                await ServerStartup.StartServer(server, formatter, registerMethods, cancellationToken);
            }
            catch (Exception ex)
            {
                RpcLog.Error("Fatal RPC server failure", ex);
                throw;
            }
        }
    }
}
