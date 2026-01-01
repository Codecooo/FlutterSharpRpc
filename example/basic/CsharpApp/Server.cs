using FlutterSharpRpc.Services;


/// <summary>
/// Class that contains the JSON-RPC methods
/// IRpcNotifierAware is only used if you plan for your server to send notification
/// if you dont then remove it
/// </summary>
public class Server : IRpcNotifierAware 
{
    // IRpcNotifier instance for sending notification
    private IRpcNotifier rpcNotifier;

    public void AttachNotifier(IRpcNotifier notifier)
    {
        rpcNotifier = notifier;
    }

    public async Task BackgroundUpdate()
    {
        if (rpcNotifier is null)
        {
            return;
        }

        int tick = 0;

        while (true)
        {
            string text = $"Update from C# server notifications. The current tick is {tick}";
            await rpcNotifier.NotifyAsync("updateProgress", text);
            RpcLog.Info($"Notifying client of the current tick {tick}");
            tick++;
            await Task.Delay(1500);
        }
    }

    public DateTime GetCurrentDateTime()
    {
        // Log to STDERR so we wont corrupt the STDOUT pipe that we using for JSON-RPC.
        RpcLog.Info($"Received 'GetCurrentDateTime' request");

        return DateTime.Now;
    }

    public int SumNumbers(int a, int b)
    {
        RpcLog.Info($"Received 'SumNumbers' request");

        return a + b;
    }

    public FilesInFolderResponse GetFilesInFolder(GetFilesInFolderRequest request)
    {
        RpcLog.Info($"Received 'GetFilesInFolder' request");

        return new FilesInFolderResponse
        {
            Files = Directory.GetFiles(request.FolderPath!).Take(10).ToArray()
        };
    }
}
