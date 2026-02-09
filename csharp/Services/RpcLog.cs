using System;
using Microsoft.Extensions.Logging;

namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// Class for logging in RPC using STDERR to not poison STDOUT used for the RPC process
    /// </summary>
    public class RpcLog : ILogger
    {
        private static string _category = "RPC";

        public RpcLog(string category)
        {
            _category = category;
        }

        public static void Info(string message)
            => Write("INF", message);

        public static void Warn(string message)
            => Write("WRN", message);

        public static void Error(string message, Exception ex = null)
        {
            Write("ERR", message);
            if (ex != null)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void Write(string level, string message)
        {
            Console.Error.WriteLine(
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] [{_category}]: {message}");
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_category.ToLowerInvariant().Contains("rpc"))
            {
                var level = LogLevelMapper(logLevel);
                Write(level, formatter(state, exception));
            }
        }

        private static string LogLevelMapper(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "TRC";
                case LogLevel.Debug:
                    return "DBG";
                case LogLevel.Information:
                    return "INF";
                case LogLevel.Warning:
                    return "WRN";
                case LogLevel.Error:
                    return "ERR";
                case LogLevel.Critical:
                    return "CRT";
                default:
                    return "UNK";
            }
        }

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public IDisposable BeginScope<TState>(TState state)
            => NullScope.Instance;
    }

    /// <summary>
    /// A null scope implementation for ILogger.BeginScope to avoid unnecessary allocations since we don't need scopes for this simple logger
    /// </summary>
    internal sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        private NullScope() { }
        public void Dispose() { }
    }
}
