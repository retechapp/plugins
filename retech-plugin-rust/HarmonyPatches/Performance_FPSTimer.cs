using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(Performance), "FPSTimer")]
public class Performance_FPSTimer
{
  [HarmonyPostfix]
  private static void Postfix() => SendPerformance.Execute();
}