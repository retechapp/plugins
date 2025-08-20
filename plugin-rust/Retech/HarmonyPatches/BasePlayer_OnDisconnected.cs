using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.OnDisconnected))]
public class BasePlayer_OnDisconnected
{
  [HarmonyPostfix]
  private static void Postfix(BasePlayer __instance)
  {
    Loader.Instance?.PlayerLeave?.OnPlayerLeave(__instance);
    Loader.Instance?.PlayerTick?.OnPlayerLeave(__instance);
  }
}
