using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class SendPerformance
{
  public static void Execute(Performance performance)
  {
    if (Loader.Instance == null)
      return;

    Performance.Tick current = Performance.current;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x0004);
    packetWriter.WriteFloat(Time.time);
    packetWriter.WriteUInt32((uint)current.frameRate);
    packetWriter.WriteUInt64((ulong)current.memoryUsageSystem);
    Loader.Instance.Send(packetWriter);
  }
}
