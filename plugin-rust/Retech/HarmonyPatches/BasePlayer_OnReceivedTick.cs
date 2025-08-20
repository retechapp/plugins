using HarmonyLib;
using Network;
using Retech;
using Retech.Features;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.OnReceivedTick))]
public class BasePlayer_OnReceivedTick
{
  [HarmonyPostfix]
  private static void Postfix(BasePlayer __instance)
  {
    Loader.Instance?.PlayerTick?.OnPlayerTick(__instance);
  }
}