using System;
using System.Diagnostics.CodeAnalysis;

namespace NekoBoiNick.Stationeers.Texture.Processor;

public static class ConsoleApp {
  [NotNull, AllowNull]
  private static Process Instance { get; set; }

  private static void Main(string[] args) {
    Console.WriteLine("Starting up.");
    Instance = new Process();
    Instance.Begin();
  }
}