using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class CollectionHelpers 
{
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

	public static int ComparePosX(MonoBehaviour a, MonoBehaviour b)
	{
		float posA = a.transform.position.x;
		float posB = b.transform.position.x;
		
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

	public static int ComparePosX(Transform a, Transform b)
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
