using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.OnReceivedVoice))]
public class BasePlayer_OnReceivedVoice
{
    [HarmonyPostfix]
    private static void Postfix(BasePlayer __instance, byte[] data) => Hook(__instance, data);

    public static void Hook(BasePlayer basePlayer, byte[] data) => SendPlayerVoice.Execute(basePlayer, data);
}
