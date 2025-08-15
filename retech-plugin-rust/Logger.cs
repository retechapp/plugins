using System;
using System.IO;
using UnityEngine;

namespace Retech;

public class Logger
{
    static Logger()
    {
        try
        {
            if (!Directory.Exists(Constants.LOG_FILE))
                Directory.CreateDirectory(Constants.LOG_FILE);

            File.AppendAllText(Constants.LOG_FILE, $"--- Logger started ---\n");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[RETECH][ERROR] Failed to initialize log file: {exception.Message}");
        }
    }

    public static void Info(string message)
    {
        Debug.Log($"[RETECH] {message}");
        WriteToFile($"[INFO] {message}");
    }

    public static void Warning(string message)
    {
        Debug.LogWarning($"[RETECH][WARNING] {message}");
        WriteToFile($"[WARNING] {message}");
    }

    public static void Error(string message, Exception? exception = null)
    {
        Debug.LogError("--------------------------------------------------");
        Debug.LogError($"[RETECH][ERROR] {message}");
        if (exception != null)
            Debug.LogError(exception.ToString());
        Debug.LogError("--------------------------------------------------");

        WriteToFile($"[ERROR] {message}{(exception != null ? $"\n  {exception.Message}" : string.Empty)}");
    }

    private static void WriteToFile(string text)
    {
        try
        {
            File.AppendAllText(Constants.LOG_FILE, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{text}\n");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[RETECH][ERROR] Failed to write to log file: {exception.Message}");
        }
    }
}
