using UnityEngine;
using System.Collections;

public static class D 
{
    public static void Log (object message)
    {
        Debug.Log(message);
//        if (Debug.isDebugBuild)
//        {
//            Debug.Log(message);
//        }
    }

    public static void LogError(object message)
    {
        if (Debug.isDebugBuild)
        {
            Debug.LogError(message);
        }
    }

    public static void LogWarning(object message)
    {
        if (Debug.isDebugBuild)
        {
            Debug.LogWarning(message);
        }
    }
}
