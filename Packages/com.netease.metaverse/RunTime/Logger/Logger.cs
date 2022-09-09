using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if !ENABLE_YUNXIN_LOGGER

public class YXUnityLog
{
    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogError(string tag, string msg)
    {
        UnityEngine.Debug.LogError(string.Format("[ERROR]{0}:{1}", tag, msg));
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogWarning(string tag, string msg)
    {
        UnityEngine.Debug.LogWarning(string.Format("[WARNING]{0}:{1}", tag, msg));
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogInfo(string tag, string msg)
    {
        UnityEngine.Debug.Log(string.Format("[INFO]{0}:{1}", tag, msg));
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public static void LogDebug(string tag, string msg)
    {
        UnityEngine.Debug.Log(string.Format("[DEBUG]{0}:{1}", tag, msg));
    }
}
#endif