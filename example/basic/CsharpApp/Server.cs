using FlutterSharpRpc.Services;
using Microsoft.Extensions.Logging;


/// <summary>
/// Class that contains the JSON-RPC methods
/// Inherit RpcNotifier class instead of IRpcNotifierAware if you plan for your server to send notifications
/// if you dont then remove it
/// </summary>
public class RpcServer(ILogger<RpcServer> logger) : RpcNotifier
{
    public async Task BackgroundUpdate()
    {
        int tick = 0;

        while (true)
        {
            string text = $"Update from C# server notifications. The current tick is {tick}";
            await JsonRpc.NotifyAsync("updateProgress", text);
            logger.LogInformation($"Notifying client of the current tick {tick}");
            tick++;
            await Task.Delay(1500);
        }
    }

    public DateTime GetCurrentDateTime()
    {
        // Log to STDERR so we wont corrupt the STDOUT pipe that we using for JSON-RPC.
        logger.LogInformation($"Received 'GetCurrentDateTime' request");

        return DateTime.Now;
    }

    public int SumNumbers(int a, int b)
    {
        logger.LogInformation($"Received 'SumNumbers' request");

        return a + b;
    }

    public FilesInFolderResponse GetFilesInFolder(GetFilesInFolderRequest request)
    {
        logger.LogInformation($"Received 'GetFilesInFolder' request");

        return new FilesInFolderResponse
        {
            Files = Directory.GetFiles(request.FolderPath!).Take(10).ToArray()
        };
    }

    public void AttachNotifier(IRpcNotifier notifier)
    {
        throw new NotImplementedException();
    }
}
