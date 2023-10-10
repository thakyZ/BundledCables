using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NekoBoiNick.Stationeers.Texture.Processor.Extensions;
internal static class FileStreamExtensions {
  /// <summary>
  /// Code borrowed from <see href="https://stackoverflow.com/a/937558/1112800"/>
  /// </summary>
  /// <param name="file">The file path to check</param>
  /// <returns>A boolean depending on if the file is locked or not</returns>
  public static bool IsFileLocked(this FileStream fileSteam) {
    try {
      using (FileStream stream = File.Open(fileSteam.Name, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        // Redundant keeping just commented for clarity.
        // stream.Close();
      }
    } catch (IOException) {
      // The file is unavailable because it is:
      // Still being written to
      // Or being processed by another thread
      // Or does not exist (has already been processed)
      return true;
    }

    // File is not locked
    return false;
  }

  public enum OutputState {
    Opened,
    Closed,
    Error
  }

  public class FileState {
    public OutputState OutputState { get; }
    public Exception? Exception { get; }
    public Type? ExceptionType { get; }

    public FileState(OutputState outputState, Exception? exception = null, Type? exceptionType = null) {
      OutputState = outputState;
      Exception = exception;
      ExceptionType = exceptionType;
    }

    public static implicit operator bool(FileState instance) => instance.OutputState == OutputState.Closed;
  }

  public static FileState WaitForFileClose(this FileStream fileSteam, int millisecondsTimeout = 1000000, int delay = 5000) {
    var isClosed = false;
    Task result = Task.Run(() => {
      while (!isClosed) {
        isClosed = !fileSteam.IsFileLocked();
        Task.Delay(delay);
      }
    });

    if (result.Wait(millisecondsTimeout)) {
      return new FileState(isClosed ? OutputState.Closed : OutputState.Opened);
    } else {
      isClosed = true;
      var exception = new TimeoutException($"Waiting for file {fileSteam.Name} to close timed out at {millisecondsTimeout} ms.");
      return new FileState(OutputState.Error, exception, exception.GetType());
    }
  }
}
