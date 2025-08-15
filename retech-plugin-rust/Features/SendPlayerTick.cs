using System;
using System.Collections.Generic;
using Network;
using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class PlayerTickUpdate(float lastTime, Vector3 lastPosition, Vector3 lastRotation, Vector3 lastVelocity)
{
  public float LastTime = lastTime;
  public Vector3 LastPosition = lastPosition;
  public Vector3 LastRotation = lastRotation;
  public Vector3 LastVelocity = lastVelocity;
}

public class SendPlayerTick
{
  private static Dictionary<ulong, PlayerTickUpdate> _playerTickUpdates = new();
  public static void Execute(BasePlayer basePlayer, Message packet)
  {
    if (Retech.Instance == null)
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

    if (Time.time - playerTickUpdate.LastTime < 0.5f)
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
    Retech.Instance.Send(packetWriter);
  }
}
