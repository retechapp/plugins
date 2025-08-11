using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.OnReceivedVoice))]
public class BasePlayer_OnReceivedVoice
{
  [HarmonyPostfix]
  private static void Postfix(BasePlayer __instance, byte[] data) => SendVoice.Execute(__instance, data);
}
