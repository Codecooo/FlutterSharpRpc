using CsharpApp;
using FlutterSharpRpc;
using FlutterSharpRpc.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        // your program setup (DI, service, etc) here...
        // start the JSON-RPC server
        // and start listening for requests coming from Dart (Flutter).

        var services = new ServiceCollection();

        // register the logging if you want to use it in your RpcServer class, otherwise you can skip this
        services.AddLogging(builder =>
        {
            // this extension will add the RpcLogProvider to the logging system, 
            // so you can inject ILogger<RpcServer> in your RpcServer class and it will log to STDERR without polluting STDOUT used for the RPC process
            // IMPORTANT: your server class category name must contain "RPC" for example here the class name is "RpcServer:
            //  so that the RpcLogProvider can log it, otherwise it will be ignored and not logged
            builder.AddRpcLogging();
        });
        services.AddSingleton<RpcServer>();

        var sp = services.BuildServiceProvider();

        var server = sp.GetRequiredService<RpcServer>();
        
        // normally you await them but for this showcase we dont. So that the below methods could be executed
        CsharpRpcServer.StartAsync(server);

        //  for aot you could start the server like this so
        // CsharpRpcServer.StartWithExplicitAsync(server, JsonContext.Default,
        //     async (rpc, server) =>
        //     {
        //         rpc.AddLocalRpcMethod("GetCurrentDateTime",server.GetCurrentDateTime);
        //         rpc.AddLocalRpcMethod("SumNumbers",server.SumNumbers);
        //         rpc.AddLocalRpcMethod("GetFilesInFolder",server.GetFilesInFolder);
        //     });

        // artificial delay so that the Server instance initialized properly normally you dont want to do this
        // only for showcase
        await Task.Delay(5000);
        await server.BackgroundUpdate();
    }
}
