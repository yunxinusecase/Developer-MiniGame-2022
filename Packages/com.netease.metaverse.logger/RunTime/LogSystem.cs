using System;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if ENABLE_YUNXIN_LOGGER
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

public class YXUnityLog
{
    static YXUnityLog()
    {
        UnityEngine.Debug.Log("Init YXUnityLog Now");
        InitLogSystem();
    }

    private static void InitLogSystem()
    {
        int processId = Process.GetCurrentProcess().Id;
        string filepath = string.Format("{0}/logs/log-{1}-{2}.txt", Application.persistentDataPath, System.DateTime.Now.ToString("yyyyMMddHHmmss"), processId.ToString());
        // Max Log File Size
        int maxSize = 1024 * 1024 * 25;
        // File Roll Count
        int fileRollCount = 10;
        Log.Logger = new LoggerConfiguration().WriteTo.File(filepath, rollOnFileSizeLimit: true, retainedFileCountLimit: fileRollCount, fileSizeLimitBytes: maxSize).MinimumLevel.Verbose().CreateLogger();

        UnityEngine.Debug.Log("InitLogSystem() called filepath:" + filepath);
        Log.Information("InitLogSystem() called filepath:" + filepath);
        // Application.logMessageReceived += UnityLogCallBack;
        // both main thread and other thread
        Application.logMessageReceivedThreaded += UnityLogCallBack;
    }

    private static void UnityLogCallBack(string condition, string stackTrack, LogType type)
    {
        Log.Information($"UNITY {type.ToString()}:{condition},{stackTrack}");
    }

    // [System.Diagnostics.Conditional("ENABLE_VERBOSE")]
    // C# Debug会默认定义DEBUG宏[点击解决方案属性->生成，查看DEBUG+TRACE定义]
    // [System.Diagnostics.Conditional("DEBUG")]
    public static void LogVerbose(string tag, string msg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(string.Format("[VERBOSE]{0}:{1}", tag, msg));
#else
        Log.Verbose(string.Format("[VERBOSE]{0}:{1}", tag, msg));
#endif
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogDebug(string tag, string msg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(string.Format("[DEBUG]{0}:{1}", tag, msg));
#else
        Log.Debug(string.Format("{0}:{1}", tag, msg));
#endif
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogInfo(string tag, string msg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(string.Format("[INFO]{0}:{1}", tag, msg));
#else
        Log.Information(string.Format("{0}:{1}", tag, msg));
#endif
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogWarning(string tag, string msg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogWarning(string.Format("[WARNING]{0}:{1}", tag, msg));
#else
        Log.Warning(string.Format("{0}:{1}", tag, msg));
#endif
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogError(string tag, string msg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogError(string.Format("[ERROR]{0}:{1}", tag, msg));
#else
        Log.Error(string.Format("{0}:{1}", tag, msg));
#endif
    }

    public static string LogsPath()
    {
        string filepath = string.Format("{0}/logs/", Application.persistentDataPath);
        return filepath;
    }
}
#endif