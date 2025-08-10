using Retech.Network;

namespace Retech.Features;

public class SendConnectionJoin
{
  public static void Execute(BasePlayer basePlayer)
  {
    if (Retech.Instance == null)
      return;

    Packet packet = new();
    packet.WriteUInt16(0x0001);
    packet.WriteUInt64(basePlayer.userID.Get());
    packet.WriteString(basePlayer.displayName);
    packet.WriteString(basePlayer.Connection.ipaddress);
    Retech.Instance.SendPacket(packet);
  }
}
