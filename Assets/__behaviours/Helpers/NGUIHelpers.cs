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
            ////D.Log(label.name + " - width: " + width);
            ////D.Log("maxWidth: " + maxWidth);
            ////D.Log("preLocalScale: " + label.transform.localScale);
            label.transform.localScale *= (maxWidth / width);
            ////D.Log("postLocalScale: " + label.transform.localScale);
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

    public static string GetLinkedSpriteName(string spriteName)
    {
        if (spriteName.Length > 0)
        {
            string newNameEnd = spriteName [spriteName.Length - 1] == 'a' ? "b" : "a";
            
            return spriteName.Substring(0, spriteName.Length - 1) + newNameEnd;
        } 
        else
        {
            return "";
        }
    }

    public static void PositionGridHorizontal(UIGrid grid, float offset = 0)
    {
        int childCount = grid.transform.childCount;

        for (int i = 0; i < grid.transform.childCount; ++i)
        {
            if(!grid.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                --childCount;
            }
        }

        grid.transform.localPosition = new Vector3(offset + -(grid.transform.childCount - 1) * grid.cellWidth / 2 * grid.transform.localScale.x,
                                                   grid.transform.localPosition.y, grid.transform.localPosition.z);
    }

    public static string GetRandomSpriteName(UIAtlas atlas, string nameMatch = null)
    {
        BetterList<string> sprites = new BetterList<string>();

        if (String.IsNullOrEmpty(nameMatch))
        {
            sprites = atlas.GetListOfSprites();
        }
        else
        {
            sprites = atlas.GetListOfSprites(nameMatch);
        }

        return sprites.size > 0 ? sprites[UnityEngine.Random.Range(0, sprites.size)] : "";
    }
}
