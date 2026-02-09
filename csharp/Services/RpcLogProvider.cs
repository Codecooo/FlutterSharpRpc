using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// A simple logger provider and logger implementation for RPC to log to STDERR without polluting STDOUT used for the RPC process
    /// </summary>
    internal class RpcLogProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
            => new RpcLog(categoryName);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // No resources to dispose
        }
    }

    public static class RpcLogBuilderExtensions
    {
        /// <summary>
        /// Extension method to add the RpcLogProvider to the logging providers, so you can see the logs in the console of your Flutter app
        /// </summary>
        public static ILoggingBuilder AddRpcLogging(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, RpcLogProvider>();
            return builder;
        }
    }
}