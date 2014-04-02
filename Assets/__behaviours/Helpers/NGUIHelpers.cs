using UnityEngine;
using System.Collections;
using System;

public static class NGUIHelpers
{
    public static void EnableUICams(bool enable = true, UICamera[] exceptions = null)
    {
        UICamera[] cams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];

        foreach (UICamera cam in cams)
        {
            if(exceptions == null || Array.IndexOf(exceptions, cam) != -1)
            {
                cam.enabled = false;
            }
        }
    }
}
