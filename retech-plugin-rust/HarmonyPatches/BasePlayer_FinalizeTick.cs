using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Retech.Features;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(BasePlayer), "FinalizeTick")]
public class BasePlayer_FinalizeTick
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        bool patched = false;

        MethodInfo fadeViolationMethod = AccessTools.Method(typeof(AntiHack), nameof(AntiHack.FadeViolations));

        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            if (patched == false && instruction.opcode == OpCodes.Call && instruction.operand == (object)fadeViolationMethod)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, typeof(BasePlayer_FinalizeTick).GetMethod(nameof(Hook)));
                patched = true;
            }
        }
    }

    public static void Hook(BasePlayer basePlayer) => SendPlayerTick.Execute(basePlayer);
}
