using Retech.Network;

namespace Retech.Features;

public class SendConnectionLeave
{
  public static void Execute(BasePlayer player)
  {
    if (Loader.Instance == null)
      return;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x0002);
    packetWriter.WriteUInt64(player.userID.Get());
    Loader.Instance.Send(packetWriter);

    SendPlayerTick.Disconnect(player.userID.Get());
  }
}
