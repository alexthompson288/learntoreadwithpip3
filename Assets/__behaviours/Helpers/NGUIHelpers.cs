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

    public static float GetLabelWidth(UILabel label)
    {
        return label.font.CalculatePrintedSize(label.text, false, UIFont.SymbolStyle.None).x * label.transform.localScale.x;
    }

    public static void MaxLabelWidth(UILabel label, float maxWidth)
    {
        float width = GetLabelWidth(label);

        if (width > maxWidth)
        {
            label.transform.localScale *= (maxWidth / width);
        }
    }
}
