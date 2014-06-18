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

    public static float GetLabelWidth(UILabel label, string text)
    {
        return label.font.CalculatePrintedSize(text, false, UIFont.SymbolStyle.None).x * label.transform.localScale.x;
    }

    public static float GetLabelHeight(UILabel label)
    {
        return label.font.CalculatePrintedSize(label.text, false, UIFont.SymbolStyle.None).y * label.transform.localScale.y;
    }

    public static float GetLabelHeight(UILabel label, string text)
    {
        return label.font.CalculatePrintedSize(text, false, UIFont.SymbolStyle.None).y * label.transform.localScale.y;
    }

    public static void MaxLabelWidth(UILabel label, float maxWidth)
    {
        float width = GetLabelWidth(label);

        if (width > maxWidth)
        {
            //Debug.Log(label.name + " - width: " + width);
            //Debug.Log("maxWidth: " + maxWidth);
            //Debug.Log("preLocalScale: " + label.transform.localScale);
            label.transform.localScale *= (maxWidth / width);
            //Debug.Log("postLocalScale: " + label.transform.localScale);
        }
    }

    public static void SetLabel(UILabel label, DataRow dr, string field, float maxWidth = -1, string preface = "")
    {
        if ( dr[field] != null )
        {
            label.text = preface + dr[field].ToString();

            if(maxWidth > 0)
            {
                MaxLabelWidth(label, maxWidth);
            }
        }
        else
        {
            label.text = "";
        }
    }
}
