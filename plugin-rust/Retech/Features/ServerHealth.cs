using System;
using System.Threading;
using Retech.Utils;

namespace Retech.Features;

public class ServerHealth : IDisposable
{
  private readonly Retech _retech;
  private readonly Timer _timer;

  public ServerHealth(Retech retech)
  {
    _retech = retech;

    _timer = new Timer(_ => OnPerformanceTick(), null, 0, 5_000);
  }

  public void Dispose()
  {
    _timer?.Dispose();
  }

  public void OnPerformanceTick()
  {
    Performance.Tick current = Performance.current;

    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      ServerHealthEvent = new ServerHealthEvent
      {
        Framerate = current.frameRate,
      }
    });
  }
}