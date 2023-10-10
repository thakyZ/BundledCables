using NekoBoiNick.Stationeers.Texture.Processor.Json;

namespace NekoBoiNick.Stationeers.Texture.Processor.Extensions;
internal static class IntExtensions {
  public static string ToHexString(this int value, string prefix = "#") {
    return $"{prefix}{value:X}";
  }
  public static HexColor ToHexColor(this int value) {
    return new HexColor(value);
  }
}
