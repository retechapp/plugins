using System.Threading;
using Retech.Features;

namespace Retech;

public class Loader : IHarmonyModHooks
{
    public static Retech? Instance;

    public void OnLoaded(OnHarmonyModLoadedArgs args)
    {
        if (Volatile.Read(ref Instance) != null)
            return;

        Retech created = null!;
        try
        {
            PluginHandshake.Request("your_token_here");

            created = new Retech();
            Retech? prev = Interlocked.CompareExchange(ref Instance, created, null);
            if (prev != null)
                created.Dispose();
        }
        catch
        {
            try { created?.Dispose(); } catch { }
            throw;
        }
    }

    public void OnUnloaded(OnHarmonyModUnloadedArgs args)
    {
        Retech? instance = Interlocked.Exchange(ref Instance, null);
        if (instance == null)
            return;

        try { instance.Dispose(); } catch { }
    }
}
