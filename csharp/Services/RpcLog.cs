using System;

namespace FlutterSharpRpc.Services
{
    #nullable enable
    /// <summary>
    /// Class for logging in RPC using STDERR to not poison STDOUT used for the RPC process
    /// </summary>
    public static class RpcLog
    {
        public static void Info(string message)
            => Write("INFO", message);

        public static void Warn(string message)
            => Write("WARN", message);

        public static void Error(string message, Exception? ex = null)
        {
            Write("ERROR", message);
            if (ex != null)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void Write(string level, string message)
        {
            Console.Error.WriteLine(
                $"[{DateTimeOffset.UtcNow:O}] [{level}] {message}");
        }
    }
}
