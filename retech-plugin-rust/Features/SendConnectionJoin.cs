using Retech.Network;

namespace Retech.Features;

public class SendConnectionJoin
{
  public static void Execute(BasePlayer basePlayer)
  {
    if (Retech.Instance == null)
      return;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x0001);
    packetWriter.WriteUInt64(basePlayer.userID.Get());
    packetWriter.WriteString(basePlayer.displayName);
    packetWriter.WriteString(basePlayer.Connection.IPAddressWithoutPort());
    Retech.Instance.Send(packetWriter);
  }
}
