using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class SendPerformance
{
  private static float _lastExecution;

  public static void Execute()
  {
    if (Loader.Instance == null)
      return;

    if (Time.time - _lastExecution < 2.5f)
      return;

    _lastExecution = Time.time;

    Performance.Tick current = Performance.current;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x0004);
    packetWriter.WriteFloat(Time.time);
    packetWriter.WriteUInt32((uint)current.frameRate);
    packetWriter.WriteUInt64((ulong)current.memoryUsageSystem);
    Loader.Instance.Send(packetWriter);
  }
}
