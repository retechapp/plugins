using System.Collections.Generic;
using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class PlayerTickUpdate(float lastTime, UnityEngine.Vector3 lastPosition, UnityEngine.Vector3 lastRotation, UnityEngine.Vector3 lastVelocity)
{
  public float LastTime = lastTime;
  public UnityEngine.Vector3 LastPosition = lastPosition;
  public UnityEngine.Vector3 LastRotation = lastRotation;
  public UnityEngine.Vector3 LastVelocity = lastVelocity;
}

public class SendPlayerTick
{
  private static Dictionary<ulong, PlayerTickUpdate> _playerTickUpdates = new();
  public static void Execute(BasePlayer basePlayer)
  {
    if (Loader.Instance == null || !Loader.Instance.Config.Features.PlayerTick)
      return;

    if (!_playerTickUpdates.TryGetValue(basePlayer.userID.Get(), out PlayerTickUpdate playerTickUpdate))
    {
      playerTickUpdate = new PlayerTickUpdate(
        Time.time,
        basePlayer.transform.position,
        basePlayer.tickViewAngles,
        basePlayer.estimatedVelocity
      );
      _playerTickUpdates.Add(basePlayer.userID.Get(), playerTickUpdate);
    }

    if (Time.time - playerTickUpdate.LastTime < 2f)
      return;

    playerTickUpdate.LastTime = Time.time;
    playerTickUpdate.LastPosition = basePlayer.transform.position;
    playerTickUpdate.LastRotation = basePlayer.tickViewAngles;
    playerTickUpdate.LastVelocity = basePlayer.estimatedVelocity;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x000A);
    packetWriter.WriteFloat(Time.time);
    packetWriter.WriteUInt64(basePlayer.userID.Get());
    packetWriter.WriteFloat(playerTickUpdate.LastPosition.x);
    packetWriter.WriteFloat(playerTickUpdate.LastPosition.y);
    packetWriter.WriteFloat(playerTickUpdate.LastPosition.z);
    packetWriter.WriteFloat(playerTickUpdate.LastRotation.x);
    packetWriter.WriteFloat(playerTickUpdate.LastRotation.y);
    packetWriter.WriteFloat(playerTickUpdate.LastRotation.z);
    packetWriter.WriteFloat(playerTickUpdate.LastVelocity.x);
    packetWriter.WriteFloat(playerTickUpdate.LastVelocity.y);
    packetWriter.WriteFloat(playerTickUpdate.LastVelocity.z);
    Loader.Instance.Send(packetWriter);
  }

  public static void Disconnect(ulong userId) => _playerTickUpdates.Remove(userId);
}
