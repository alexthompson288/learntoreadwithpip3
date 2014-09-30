using UnityEngine;
using System.Collections;
using System;

public static class NGUIHelpers
{
    public static void EnableUICams(bool enable, UICamera[] exceptions = null)
    {
        //D.Log(string.Format("NGUIHelpers.EnableUICams({0})", enable));
        UICamera[] cams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];

        foreach (UICamera cam in cams)
        {
            if(exceptions == null || Array.IndexOf(exceptions, cam) != -1)
            {
                cam.enabled = enable;
            }
        }
    }

    public static Vector3 GetLabelSize3(UILabel label)
    {
        Vector2 size = label.printedSize;
        size.x *= label.transform.localScale.x;
        size.y *= label.transform.localScale.y;

        return new Vector3(size.x, size.y, label.transform.localScale.z);
    }

    public static float GetLabelWidth(UILabel label)
    {
        return label.printedSize.x;
    }

    public static float GetLabelWidth(UILabel label, string text)
    {
        string originalText = label.text;
        label.text = text;
        float width = label.printedSize.x * label.transform.localScale.x;
        label.text = originalText;
        return width;
    }

    public static float GetLabelHeight(UILabel label)
    {
        return label.printedSize.y;
    }

    public static float GetLabelHeight(UILabel label, string text)
    {
        string originalText = label.text;
        label.text = text;
        float height = label.printedSize.y * label.transform.localScale.y;
        label.text = originalText;
        return height;
    }

    public static void MaxLabelWidth(UILabel label, float maxWidth)
    {
        float width = GetLabelWidth(label);

        if (width > maxWidth)
        {
            //////////D.Log(label.name + " - width: " + width);
            //////////D.Log("maxWidth: " + maxWidth);
            //////////D.Log("preLocalScale: " + label.transform.localScale);
            label.transform.localScale *= (maxWidth / width);
            //////////D.Log("postLocalScale: " + label.transform.localScale);
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
