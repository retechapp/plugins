using System.Collections.Concurrent;
using Retech.Utils;
using UnityEngine;

namespace Retech.Features;


public class PlayerTick(Retech retech)
{
  private readonly Retech _retech = retech;

  private class PlayerTickUpdate(float lastTime, UnityEngine.Vector3 lastPosition, UnityEngine.Vector3 lastRotation, UnityEngine.Vector3 lastVelocity)
  {
    public float LastTime = lastTime;
    public UnityEngine.Vector3 LastPosition = lastPosition;
    public UnityEngine.Vector3 LastRotation = lastRotation;
    public UnityEngine.Vector3 LastVelocity = lastVelocity;
  }

  private static ConcurrentDictionary<string, PlayerTickUpdate> _playerTickUpdates = new();

  public void OnPlayerTick(BasePlayer basePlayer)
  {
    if (!_playerTickUpdates.TryGetValue(basePlayer.UserIDString, out PlayerTickUpdate playerTickUpdate))
    {
      playerTickUpdate = new PlayerTickUpdate(
        Time.time,
        basePlayer.transform.position,
        basePlayer.tickViewAngles,
        basePlayer.estimatedVelocity
      );
      _playerTickUpdates.TryAdd(basePlayer.UserIDString, playerTickUpdate);
    }

    if (Time.time - playerTickUpdate.LastTime < 2f)
      return;

    playerTickUpdate.LastTime = Time.time;
    playerTickUpdate.LastPosition = basePlayer.transform.position;
    playerTickUpdate.LastRotation = basePlayer.tickViewAngles;
    playerTickUpdate.LastVelocity = basePlayer.estimatedVelocity;

    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      PlayerTickEvent = new PlayerTickEvent
      {
        PlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Identifier = basePlayer.UserIDString,
        },
        Position = new Vector3
        {
          X = playerTickUpdate.LastPosition.x,
          Y = playerTickUpdate.LastPosition.y,
          Z = playerTickUpdate.LastPosition.z
        },
        Rotation = new Vector3
        {
          X = playerTickUpdate.LastRotation.x,
          Y = playerTickUpdate.LastRotation.y,
          Z = playerTickUpdate.LastRotation.z
        },
        Velocity = new Vector3
        {
          X = playerTickUpdate.LastVelocity.x,
          Y = playerTickUpdate.LastVelocity.y,
          Z = playerTickUpdate.LastVelocity.z
        }
      }
    });
  }

  public void OnPlayerLeave(BasePlayer basePlayer)
  {
    _playerTickUpdates.TryRemove(basePlayer.UserIDString, out _);
  }
}