using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class CollectionHelpers 
{
    public static void DestroyObjects<T>(IList<T> list, bool sendOffMessage = false)
    {
        bool hasDestroyed = false;

        for (int i = list.Count - 1; i > -1; --i)
        {
            Component comp = list[i] as Component;

            GameObject go = comp != null ? comp.gameObject : list[i] as GameObject;

            if(go != null)
            {
                if(sendOffMessage)
                {
                    go.SendMessage("Off");
                }
                else
                {
                    MonoBehaviour.Destroy(go);
                }

                hasDestroyed = true;
            }
        }

        if (hasDestroyed)
        {
            list.Clear();
        }
    }

	public static void Shuffle<T>(IList<T> list)
	{
		System.Random rng = new System.Random();
		int n = list.Count;
		while(n > 1)
		{
			--n;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

    public static string ConcatList<T>(IList<T> list)
    {
        string concat = "";

        foreach (T t in list) 
        {
            concat += String.Format("{0}_", t);
        }
        
        concat = concat.TrimEnd (new char[] { '_' });
        
        return concat;
    }

    public static bool IsSubset<T>(IList<T> subset, IList<T> superset)
    {
        return !subset.Except(superset).Any();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Comparisons for sorting Collections

	public static int LeftToRight(MonoBehaviour a, MonoBehaviour b)
	{
        return LeftToRight(a.transform, b.transform);
	}

	public static int LeftToRight(Transform a, Transform b)
	{
		float posA = a.position.x;
		float posB = b.position.x;

		if(posA < posB)
		{
			return -1;
		}
		else if(posA > posB)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}

    public static int LocalLeftToRight(MonoBehaviour a, MonoBehaviour b)
    {
        return LocalLeftToRight(a.transform, b.transform);
    }

    public static int LocalLeftToRight(Transform a, Transform b)
    {
        float posA = a.localPosition.x;
        float posB = b.localPosition.x;
        
        if(posA < posB)
        {
            return -1;
        }
        else if(posA > posB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public static int TopToBottom(MonoBehaviour a, MonoBehaviour b)
    {
        return LeftToRight(a.transform, b.transform);
    }
    
    public static int TopToBottom(Transform a, Transform b)
    {
        float posA = a.position.y;
        float posB = b.position.y;
        
        if(posA < posB)
        {
            return -1;
        }
        else if(posA > posB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public static int TopToBottomThenLeftToRight(MonoBehaviour a, MonoBehaviour b)
    {
        return TopToBottomThenLeftToRight(a.transform, b.transform);
    }

    public static int TopToBottomThenLeftToRight(Transform a, Transform b)
    {
        if (a == null && b == null)
        {
            return 0;
        } 
        else if (a == null)
        {
            return 1;
        } 
        else if (b == null)
        {
            return -1;
        } 
        else
        {
            float aX = a.position.x;
            float aY = a.position.y;
            float bX = b.position.x;
            float bY = b.position.y;

            if (Mathf.Approximately(aY, bY))
            {
                //D.Log("yEqual: " + a.name + " - " + b.name);
                if (Mathf.Approximately(aX, bX))
                {
                    return 0;
                }
                if (aX < bX)
                {
                    return -1;
                } 
                else
                {
                    return 1;
                }
            } 
            else if (aY > bY)
            {
                return -1;
            } 
            else
            {
                return 1;
            }
        }
    }

    public static int CompareName(UnityEngine.Object a, UnityEngine.Object b)
    {
        return String.Compare(a.name, b.name);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/*
	public static bool AreEqual<T>(IList<T> list1, IList<T> list2)
	{
		bool areEqual = false;

		if(list1.Count == list2.Count)
		{
			areEqual = true;

			T[] copy1 = new T[list1.Count];
			list1.CopyTo(copy1);
			Array.Sort(copy1);
			
			T[] copy2 = new T[list2.Count];
			list2.CopyTo(copy2);
			Array.Sort(copy2);


			for(int i = 0; i < copy1.Count; ++i)
			{
				if(copy1[i] != copy1[i])
				{
					areEqual = false;
					break;
				}
			}
		}

		return areEqual;
	}
	*/
}
