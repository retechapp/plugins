using ConVar;
using HarmonyLib;

namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(Chat), "sayAs")]
public class Chat_SayAs
{
    [HarmonyPostfix]
    private static void Postfix(Chat.ChatChannel targetChannel, ulong userId, string username, string message)
    {
        Loader.Instance?.PlayerChat?.OnReceivedChat(targetChannel, userId, username, message);
    }
}
