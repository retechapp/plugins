using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Retech.Utils;

public static class TimeUtils
{
  private static readonly long EpochMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  private static readonly long StartTimestamp = Stopwatch.GetTimestamp();
  private static readonly double TickFrequency = 1000.0 / Stopwatch.Frequency;

  public static long UnixTimeMilliseconds()
  {
    long elapsed = Stopwatch.GetTimestamp() - StartTimestamp;
    return EpochMilliseconds + (long)(elapsed * TickFrequency);
  }

  public static long UnixTimeSeconds()
  {
    return UnixTimeMilliseconds() / 1000;
  }
}