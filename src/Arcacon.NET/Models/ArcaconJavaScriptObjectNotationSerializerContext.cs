using System.Text.Json.Serialization;

namespace Arcacon.NET.Models;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<PublicArcaconStickerPayload>))]
internal sealed partial class ArcaconJavaScriptObjectNotationSerializerContext : JsonSerializerContext
{
}
