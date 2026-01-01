using FlutterSharpRpc;

internal class Program
{
    static async Task Main(string[] args)
    {
        // your program setup (DI, service, etc) here...
        // start the JSON-RPC server
        // and start listening for requests coming from Dart (Flutter).

        var server = new Server();
        // normally you await them but for this showcase we dont. So that the below methods could be executed
        CsharpRpcServer.StartAsync(server);

        //  for aot you could start the server like this so
        // CsharpRpcServer.StartWithExplicitAsync(server,
        //     JsonContext.Default,
        //     async (rpc, server) =>
        //     {
        //         rpc.AddLocalRpcMethod(
        //             "GetCurrentDateTime",
        //             server.GetCurrentDateTime
        //         );
        //         rpc.AddLocalRpcMethod(
        //             "SumNumbers",
        //             server.SumNumbers
        //         );
        //         rpc.AddLocalRpcMethod(
        //             "GetFilesInFolder",
        //             server.GetFilesInFolder
        //         );
        //     });

        // artificial delay so that the Server instance initialized properly normally you dont want to do this
        // only for showcase
        await Task.Delay(5000);
        await server.BackgroundUpdate();
    }
}
