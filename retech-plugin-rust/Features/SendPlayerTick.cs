using System.Collections.Generic;
using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class PlayerTickUpdate(float lastTime, Vector3 lastPosition, Vector3 lastRotation, Vector3 lastVelocity)
{
    public float lastTime = lastTime;
    public Vector3 lastPosition = lastPosition;
    public Vector3 lastRotation = lastRotation;
    public Vector3 lastVelocity = lastVelocity;
}

public class SendPlayerTick
{
    private static readonly Dictionary<ulong, PlayerTickUpdate> playerTickUpdates = [];

    public static void Execute(BasePlayer basePlayer)
    {
        if (Retech.Instance == null)
            return;

        ulong steamId = basePlayer.userID.Get();

        if (!playerTickUpdates.TryGetValue(steamId, out PlayerTickUpdate playerTickUpdate))
            playerTickUpdates.Add(steamId, new PlayerTickUpdate(Time.time, basePlayer.transform.position, basePlayer.transform.rotation.eulerAngles, basePlayer.estimatedVelocity));

        if (playerTickUpdate.lastTime - Time.time < 0.2f)
            return;

        if (playerTickUpdate.lastPosition == basePlayer.transform.position && playerTickUpdate.lastRotation == basePlayer.transform.rotation.eulerAngles && playerTickUpdate.lastVelocity == basePlayer.estimatedVelocity)
            return;

        playerTickUpdate.lastPosition = basePlayer.transform.position;
        playerTickUpdate.lastRotation = basePlayer.transform.rotation.eulerAngles;
        playerTickUpdate.lastVelocity = basePlayer.estimatedVelocity;

        Packet packet = new();
        packet.WriteUInt16(0x000A);
        packet.WriteFloat(Time.time);
        packet.WriteUInt64(steamId);
        packet.WriteFloat(basePlayer.transform.position.x);
        packet.WriteFloat(basePlayer.transform.position.y);
        packet.WriteFloat(basePlayer.transform.position.z);
        packet.WriteFloat(basePlayer.transform.rotation.x);
        packet.WriteFloat(basePlayer.transform.rotation.y);
        packet.WriteFloat(basePlayer.transform.rotation.z);
        packet.WriteFloat(basePlayer.estimatedVelocity.x);
        packet.WriteFloat(basePlayer.estimatedVelocity.y);
        packet.WriteFloat(basePlayer.estimatedVelocity.z);
        Retech.Instance.SendPacket(packet);
    }
}
