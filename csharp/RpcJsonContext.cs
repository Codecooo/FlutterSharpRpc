#if NET8_0_OR_GREATER
using System;
using System.Text.Json.Serialization;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace FlutterSharpRpc
{
    /// <summary>
    /// JsonSerializerContext source generation for base class used by StreamRpcJson
    /// </summary>
    [JsonSerializable(typeof(RequestId))]
    [JsonSerializable(typeof(JsonRpcRequest))]
    [JsonSerializable(typeof(JsonRpcError))]
    [JsonSerializable(typeof(CommonErrorData))]
    [JsonSerializable(typeof(JsonRpcResult))]
    [JsonSerializable(typeof(JsonRpcMessage))]
    [JsonSerializable(typeof(DateTime))]
    internal partial class RpcJsonContext : JsonSerializerContext
    {
    }
}
#endif