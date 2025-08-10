using Retech.Network;

namespace Retech.Features;

public class SendConnectionLeave
{
  public static void Execute(BasePlayer player)
  {
    if (Retech.Instance == null)
      return;

    Packet packet = new();
    packet.WriteUInt16(0x0002);
    packet.WriteUInt64(player.userID.Get());
    Retech.Instance.SendPacket(packet);
  }
}
