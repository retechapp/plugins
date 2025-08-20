using System.Diagnostics;

namespace Retech.Utils;

public static class TimeUtils
{
  private static readonly long StartTimestamp = Stopwatch.GetTimestamp();

  public static long UnixTimeMilliseconds() => (Stopwatch.GetTimestamp() - StartTimestamp) * 1000L / Stopwatch.Frequency;

  public static long UnixTimeSeconds() => UnixTimeMilliseconds() / 1000;
}