using System;
using System.IO;
using UnityEngine;

namespace Retech;

public class Logger
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    static bool _unityLogger = true;
    static Logger()
    {
        try
        {
            string directoryName = Path.GetDirectoryName(Constants.LOG_FILE);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.AppendAllText(Constants.LOG_FILE, $"--- Logger started ---\n");
        }
        catch (Exception exception)
        {
            if (_unityLogger)
                Debug.LogError($"[RETECH][ERROR] Failed to initialize log file: {exception.Message}");
            else
                Console.WriteLine($"[RETECH][ERROR] Failed to initialize log file: {exception.Message}");
        }
    }

    public static void DisableUnityLogger() => _unityLogger = false;

    public static void Log(LogLevel level, string section, string message, Exception? exception = null)
    {
        string exceptionText = exception != null ? $"\n{exception}" : "";

        string text = $"[{level switch
        {
            LogLevel.Info => "INFO",
            LogLevel.Warning => "WARNING",
            LogLevel.Error => "ERROR",
            _ => throw new NotImplementedException()
        }}]{(string.IsNullOrEmpty(section) ? "" : $"[{section}]")} {message}{exceptionText}";

        switch (level)
        {
            case LogLevel.Info:
                if (_unityLogger)
                    Debug.Log($"[RETECH]{text}");
                else
                    Console.WriteLine($"[RETECH]{text}");
                break;

            case LogLevel.Warning:
                if (_unityLogger)
                    Debug.LogWarning($"[RETECH]{text}");
                else
                    Console.WriteLine($"[RETECH]{text}");
                break;

            case LogLevel.Error:
                if (_unityLogger)
                    Debug.LogError($"--------------------------------------------------\n[RETECH]{text}\n--------------------------------------------------");
                else
                    Console.WriteLine($"--------------------------------------------------\n[RETECH]{text}\n--------------------------------------------------");
                break;
        }

        WriteToFile(text);
    }

    public static void Info(string message) => Log(LogLevel.Info, string.Empty, message);

    public static void Warning(string message) => Log(LogLevel.Warning, string.Empty, message);

    public static void Error(string message, Exception? exception = null) => Log(LogLevel.Error, string.Empty, message, exception);

    private static void WriteToFile(string text)
    {
        try
        {
            File.AppendAllText(Constants.LOG_FILE, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{text}\n");
        }
        catch (Exception exception)
        {
            if (_unityLogger)
                Debug.LogError($"[RETECH][ERROR] Failed to write to log file: {exception.Message}");
            else
                Console.WriteLine($"[RETECH][ERROR] Failed to write to log file: {exception.Message}");
        }
    }
}
