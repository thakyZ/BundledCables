using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using NekoBoiNick.Stationeers.Texture.Processor.Extensions;
using NekoBoiNick.Stationeers.Texture.Processor.Json;

using SkiaSharp;

using Svg;
using Svg.Skia;

namespace NekoBoiNick.Stationeers.Texture.Processor;

internal partial class Process {
  private const string drawingSVG = "drawing.svg";
  private const string colorsData = "colors.json";
  private const string outputPng = "output.png";
  private string svgFile = string.Empty;
  private SKBitmap? currentBitmap;

  private DirectoryInfo ProjectDirectory { get; }

  public Process() {
    ProjectDirectory = GetProjectDirectory();
  }

  private SKBitmap? StitchImage(Grid grid, ColorMap colorsMap, SKBitmap skOverlay) {
    try {
      var canvas = new SKCanvas(currentBitmap);
      canvas.DrawBitmap(skOverlay, new SKPoint(grid.Width * colorsMap.Position.X, grid.Height * colorsMap.Position.Y));
      canvas.Save();
      return currentBitmap;
    } catch (Exception exception) {
      Console.WriteLine($"Soup \"{exception.Message}\"");
      throw;
    }
  }

  private SKBitmap ProcessColors(Colors colors, string tempDirectory = "", int index = -1) {
    var alteredSvgFile = svgFile;

    var lines = NewLineRegex().Split(alteredSvgFile).ToList();
    var rootNamespace = lines.FindIndex(x => RootNamespaceRegex().IsMatch(x));
    List<Match> varLines = lines
      .Where((x, index) => {
        var red = RedVarRegex().IsMatch(x);
        var green = GreenVarRegex().IsMatch(x);
        var blue = BlueVarRegex().IsMatch(x);
        var rot = RotationVarRegex().IsMatch(x);
        return (red || green || blue || rot);
      }).Select((x) => {
        var red = RedVarRegex().IsMatch(x);
        var green = GreenVarRegex().IsMatch(x);
        var blue = BlueVarRegex().IsMatch(x);
        var rot = RotationVarRegex().IsMatch(x);
        if (red)
          return RedVarRegex().Match(x);
        if (green)
          return GreenVarRegex().Match(x);
        if (blue)
          return BlueVarRegex().Match(x);
        if (rot)
          return RotationVarRegex().Match(x);
        return null!;
      }).ToList();

    foreach (Match line in varLines) {
      if (line.Captures.Count == 0)
        continue;

      if (line.Value.Contains("red")) {
        alteredSvgFile = alteredSvgFile.Replace($"{line.Value}", $"#{colors.Red.ToHexString()}");
      } else if (line.Value.Contains("green")) {
        alteredSvgFile = alteredSvgFile.Replace($"{line.Value}", $"#{colors.Green.ToHexString()}");
      } else if (line.Value.Contains("blue")) {
        alteredSvgFile = alteredSvgFile.Replace($"{line.Value}", $"#{colors.Blue.ToHexString()}");
      } else if (line.Value.Contains("rotation")) {
        alteredSvgFile = alteredSvgFile.Replace($"{line.Value}", "-45deg");
      }
    }

    SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(alteredSvgFile);

    svgDocument.GetElementById("group").Transforms.Add(new Svg.Transforms.SvgRotate(-0.45f, 128, 128));
    svgDocument.GetElementById("red-def").Transforms.Add(new Svg.Transforms.SvgScale(1.0f, 3 / 9 * 256));
    svgDocument.GetElementById("red-def").Transforms.Add(new Svg.Transforms.SvgTranslate(0.0f, 3 / 9 * 0.0f));
    svgDocument.GetElementById("green-def").Transforms.Add(new Svg.Transforms.SvgScale(1.0f, 3 / 9 * 256));
    svgDocument.GetElementById("green-def").Transforms.Add(new Svg.Transforms.SvgTranslate(0.0f, 3 / 9 * (30 / 10)));
    svgDocument.GetElementById("blue-def").Transforms.Add(new Svg.Transforms.SvgScale(1.0f, 3 / 9 * 256));
    svgDocument.GetElementById("blue-def").Transforms.Add(new Svg.Transforms.SvgTranslate(0.0f, 3 / 9 * (60 / 10)));


    SKPicture? svg = new SKSvg().FromSvgDocument(svgDocument);
    SKBitmap? picture = svg!.ToBitmap(SKColor.Empty, 1, 1, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear()) ?? throw new NullReferenceException("Failed to load altered SVG file");

#if DEBUG
    var output = SvgDocument.FromSvg<SvgDocument>(alteredSvgFile);

    using (FileStream fileOutStream = new FileStream(Path.Join(tempDirectory, $"{drawingSVG.Split('.')[0]}_{index}.png"), FileMode.Create, FileAccess.Write, FileShare.Write, default, true)) {
      var fs = new SKManagedWStream(fileOutStream);
      picture.Encode(fs, SKEncodedImageFormat.Png, quality: 100);
      fileOutStream.Close();
      fileOutStream.WaitForFileClose();
    }

    File.WriteAllText(Path.Join(tempDirectory, $"{drawingSVG.Split('.')[0]}_{index}.svg"), alteredSvgFile);

    return picture;
# else
    return picture;
#endif
  }

  public void Begin() {
    if (ProjectDirectory is not null && File.Exists(Path.Join(ProjectDirectory.FullName, drawingSVG)) && File.Exists(Path.Join(ProjectDirectory.FullName, colorsData))) {
      var options = new JsonSerializerOptions() { AllowTrailingCommas = true };
      options.Converters.Add(new HexStringToHexColorConverter());
      ColorsJsonSchema? jsonData = JsonSerializer.Deserialize<ColorsJsonSchema>(File.ReadAllText(Path.Join(ProjectDirectory.FullName, colorsData)), options) ?? throw new NullReferenceException("Reading the \"colors.json\" file returned a null value.");
      var tempDirectory = Path.Join(ProjectDirectory.FullName, "temp");

      if (Directory.Exists(tempDirectory))
      {
        Directory.Delete(tempDirectory, true);
      }

      Directory.CreateDirectory(tempDirectory);
      svgFile = File.ReadAllText(Path.Join(ProjectDirectory.FullName, drawingSVG));

      if (File.Exists(Path.Join(tempDirectory, $"{jsonData.Image.Split('.')[0]}_0.png"))) {
        File.Delete(Path.Join(tempDirectory, $"{jsonData.Image.Split('.')[0]}_0.png"));
      }

      var index = 0;

      using (FileStream fileInStream = new FileStream(Path.Join(ProjectDirectory.FullName, jsonData.Image), FileMode.Open, FileAccess.Read, FileShare.Read, default, true))
      using (MemoryStream memoryStream = new MemoryStream()) {
        fileInStream.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        currentBitmap = SKBitmap.Decode(memoryStream);
        memoryStream.Close();
        fileInStream.Close();
      }
      foreach (ColorMap colorsMap in jsonData.ColorMap) {
        index = jsonData.ColorMap.FindIndex(x => x.Colors.Red.Equals(colorsMap.Colors.Red) && x.Colors.Green.Equals(colorsMap.Colors.Green) && x.Colors.Blue.Equals(colorsMap.Colors.Blue));
        Console.WriteLine($"{index}/{jsonData.ColorMap.Count}");
# if DEBUG
        var bitmap = ProcessColors(colorsMap.Colors, tempDirectory, index);
# else
        var bitmap = ProcessColors(colorsMap.Colors);
#endif
        currentBitmap = StitchImage(jsonData.Grid, colorsMap, bitmap);
      }
      using (FileStream fileOutStream = new FileStream(Path.Join(ProjectDirectory.FullName, outputPng), FileMode.Create, FileAccess.Write, FileShare.Write, default, true)) {
        var fs = new SKManagedWStream(fileOutStream);
        currentBitmap.Encode(fs, SKEncodedImageFormat.Png, quality: 100);
        fileOutStream.Close();
        var state = fileOutStream.WaitForFileClose();

        if (state.OutputState == FileStreamExtensions.OutputState.Error) {
          throw state.Exception!;
        } else if (state.OutputState == FileStreamExtensions.OutputState.Opened) {
          throw state.Exception!;
        }
      }
    } else {
      if (ProjectDirectory is null) {
        Console.WriteLine("ProjectDirectory is null.");
      } else {
        if (!File.Exists(Path.Join(ProjectDirectory.FullName, drawingSVG))) {
          Console.WriteLine($"Path, `{Path.Join(ProjectDirectory.FullName, drawingSVG)}` not found.");
        }

        if (!File.Exists(Path.Join(ProjectDirectory.FullName, colorsData))) {
          Console.WriteLine($"Path, `{Path.Join(ProjectDirectory.FullName, colorsData)}` not found.");
        }
      }

      throw new InvalidOperationException("See error(s) above for the issues.");
    }
  }

  private static DirectoryInfo GetExecutableDirectory() => new FileInfo(Assembly.GetExecutingAssembly().FullName!).Directory!;

  private static DirectoryInfo GetProjectDirectory() {
    var currentDirectory = GetExecutableDirectory();
    while (currentDirectory.GetFiles("*.csproj").Length == 0) {
      if (RootRegex().IsMatch(currentDirectory.FullName)) {
        throw new FileNotFoundException($"No file found as a parent to {GetExecutableDirectory().FullName} that contains pattern \"*.csproj\".");
      }

      currentDirectory = currentDirectory.Parent!;
    }
    return currentDirectory;
  }

  [GeneratedRegex(@"^ *rect {$")]
  private static partial Regex RootNamespaceRegex();

  [GeneratedRegex(@"var\(--red\)")]
  private static partial Regex RedVarRegex();

  [GeneratedRegex(@"var\(--green\)")]
  private static partial Regex GreenVarRegex();

  [GeneratedRegex(@"var\(--blue\)")]
  private static partial Regex BlueVarRegex();

  [GeneratedRegex(@"var\(--rotation\)")]
  private static partial Regex RotationVarRegex();
  [GeneratedRegex(@"(?:\r)?\n")]
  private static partial Regex NewLineRegex();
  [GeneratedRegex(@"^\w:\\$")]
  private static partial Regex RootRegex();
}