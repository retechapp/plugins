namespace Retech;

public class Loader : IHarmonyModHooks
{
    public void OnLoaded(OnHarmonyModLoadedArgs args)
    {
        if (Retech.Instance != null)
            return;

        Retech.Instance = new Retech();
    }

    public void OnUnloaded(OnHarmonyModUnloadedArgs args)
    {
        if (Retech.Instance == null)
            return;

        Retech.Instance.Dispose();
        Retech.Instance = null;
    }
}
