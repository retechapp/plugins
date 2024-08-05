using System;
using System.IO;
using UnityEngine;

namespace Retech;

public class Logger
{
    public static StreamWriter streamWriter = new(Constants.LOG_FILE, true);

    public static void Info(string message)
    {
        Debug.Log($"[RETECH] {message}");

        streamWriter.WriteLine($"[INFO] {message}");
        streamWriter.FlushAsync();
    }

    public static void Warning(string message)
    {
        Debug.LogWarning($"[RETECH][WARNING] {message}");

        streamWriter.WriteLine($"[WARNING] {message}");
        streamWriter.FlushAsync();
    }

    public static void Error(string message, Exception? exception = null)
    {
        Debug.LogError($"[RETECH][ERROR] {message}");

        streamWriter.WriteLine($"[ERROR] {message}");
        streamWriter.FlushAsync();
    }

    public static void Setup()
    {
        FileInfo fileInfo = new(Constants.LOG_FILE);

        if (!fileInfo.Directory.Exists)
            fileInfo.Directory.Create();
    }

    public static void Close()
    {
        streamWriter.Close();
    }
}
