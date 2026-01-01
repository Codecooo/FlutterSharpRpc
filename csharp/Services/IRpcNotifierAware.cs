namespace FlutterSharpRpc.Services
{
    /// <summary>
    /// Provide a base implementation for a class to be able to notify others through RPC
    /// </summary>
    public interface IRpcNotifierAware
    {
        /// <summary>
        /// Attach notifier to the class so they can start notify others
        /// </summary>
        /// <param name="notifier">The notifier instance that implements IRpcNotifier</param>
        void AttachNotifier(IRpcNotifier notifier);
    }
}