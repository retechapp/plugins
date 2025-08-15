using HarmonyLib;
using Network;
using Retech.Features;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.OnReceivedTick))]
public class BasePlayer_OnReceivedTick
{
  [HarmonyPostfix]
  private static void Postfix(BasePlayer __instance, Message packet) => SendPlayerTick.Execute(__instance, packet);
}