using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using NekoBoiNick.Stationeers.Texture.Processor.Extensions;

namespace NekoBoiNick.Stationeers.Texture.Processor.Json;

[Serializable]
public class ColorsJsonSchema {
  public string Image { get; set; } = string.Empty;

  [NotNull, AllowNull]
  public Grid Grid { get; set; }

  [NotNull, AllowNull]
  public List<ColorMap> ColorMap { get; set; }
}

[Serializable]
public class Grid {
  public int Width { get; set; } = 16;
  public int Height { get; set; } = 16;
}

[Serializable]
public class ColorMap {
  [NotNull, AllowNull]
  public Position Position { get; set; }

  [NotNull, AllowNull]
  public Colors Colors { get; set; }
}

[Serializable]
public class Position {
  public int X { get; set; } = 0;
  public int Y { get; set; } = 0;
}

[Serializable]
public class Colors {
  [JsonConverter(typeof(HexStringToHexColorConverter))]
  public HexColor Red { get; set; } = 0x000000.ToHexColor();
  [JsonConverter(typeof(HexStringToHexColorConverter))]
  public HexColor Green { get; set; } = 0x000000.ToHexColor();
  [JsonConverter(typeof(HexStringToHexColorConverter))]
  public HexColor Blue { get; set; } = 0x000000.ToHexColor();
}