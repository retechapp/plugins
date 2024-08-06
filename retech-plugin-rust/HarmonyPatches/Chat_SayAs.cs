using ConVar;
using HarmonyLib;


namespace Retech.HarmonyPatches;

[HarmonyPatch(typeof(Chat), "sayAs")]
public class Chat_SayAs
{
    [HarmonyPostfix]
    private static void Postfix(Chat.ChatChannel targetChannel, ulong userId, string username, string message) => Hook(userId, targetChannel, message);

    public static void Hook(ulong userId, Chat.ChatChannel targetChannel, string message) => Features.SendPlayerChat.Execute(userId, targetChannel, message);
}
