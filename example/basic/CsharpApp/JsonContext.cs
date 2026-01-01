using System.Text.Json.Serialization;

namespace CsharpApp;

[JsonSerializable(typeof(GetFilesInFolderRequest))]
[JsonSerializable(typeof(FilesInFolderResponse))]
public partial class JsonContext : JsonSerializerContext;