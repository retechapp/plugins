namespace Retech;

public class Loader : IHarmonyModHooks
{
    public void OnLoaded(OnHarmonyModLoadedArgs args)
    {
        Retech.Instance = new Retech();
    }

    public void OnUnloaded(OnHarmonyModUnloadedArgs args)
    {
        Retech.Instance?.Dispose();
    }
}
