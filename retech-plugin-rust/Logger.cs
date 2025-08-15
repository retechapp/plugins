using System;
using UnityEngine;

namespace Retech;

public class Logger
{
    public static void Info(string message)
    {
        Debug.Log($"[RETECH] {message}");
    }

    public static void Warning(string message)
    {
        Debug.LogWarning($"[RETECH][WARNING] {message}");
    }

    public static void Error(string message, Exception? exception = null)
    {
        Debug.LogError("--------------------------------------------------");
        Debug.LogError($"[RETECH][ERROR] {message}");
        if (exception != null)
            Debug.LogError(exception.ToString());
        Debug.LogError("--------------------------------------------------");
    }
}
