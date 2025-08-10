using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.PlayerInit))]
public class BasePlayer_PlayerInit
{
  [HarmonyPostfix]
  private static void Postfix(BasePlayer __instance) => SendConnectionJoin.Execute(__instance);
}
