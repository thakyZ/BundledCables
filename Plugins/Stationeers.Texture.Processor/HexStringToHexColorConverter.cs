using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NekoBoiNick.Stationeers.Texture.Processor.Extensions;

namespace NekoBoiNick.Stationeers.Texture.Processor.Json;

internal class HexStringToHexColorConverter : JsonConverter<HexColor> {
  public override HexColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
    var output = Convert.ToInt32(reader.GetString(), 16);
    return output.ToHexColor();
  }

  public override void Write(Utf8JsonWriter writer, HexColor value, JsonSerializerOptions options) =>
    writer.WriteStringValue($"{value:X}");
}

public class HexColor {
  public HexColor(int value) {
    Value = value;
  }

  private int Value { get; set; }

  public override string ToString() =>
      Value.ToString();

  public string ToString(IFormatProvider? provider) =>
      Value.ToString(provider);

  public string ToString(string? format) =>
      Value.ToString(format);

  public string ToHexString(string prefix = "") {
    var temp = "";
    if ($"{Value:X}".Length < 6) {
      temp = new string('0', 6 - $"{Value:X}".Length);
    }
    return $"{prefix}{temp:X}{Value:X}";
  }

  public string ToString(string? format, IFormatProvider? provider) =>
      Value.ToString(format, provider);

  public static implicit operator int(HexColor instance) {
    return instance.Value;
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "Don't need a parameter but it is required.")]
  public int this[object? none] {
    get => Value;
    set => Value = value;
  }
}