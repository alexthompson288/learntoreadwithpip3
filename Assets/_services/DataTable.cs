using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class DataRow : Dictionary<string, object>, IEquatable<DataRow>
{
    public new object this[string column]
    {
        get
        {
            if (ContainsKey(column))
            {
                return base[column];
            }
            
            return null;
        }
        set
        {
            if (ContainsKey(column))
            {
                base[column] = value;
            }
            else
            {
                Add(column, value);
            }
        }
    }

	public bool Equals(DataRow other)
	{
		if(other == null)
		{
			return false;
		}
		else
		{
			return (Convert.ToInt32(this["id"]) == Convert.ToInt32(other["id"]));
		}
	}

	public override bool Equals(System.Object obj)
	{
		Debug.Log("Override Equals");
		DataRow other = obj as DataRow;

		if(other == null)
		{
			return false;
		}
		else
		{
			return (Convert.ToInt32(this["id"]) == Convert.ToInt32(other["id"]));
		}
	}

	public override int GetHashCode()
	{
		return Convert.ToInt32(this["id"]);
	}
}

public class SectionComparer : IComparer<DataRow>
{
	public int Compare(DataRow a, DataRow b)
	{
		//Debug.Log("Compare");
		int idA = System.Convert.ToInt32(a["id"]);
		int idB = System.Convert.ToInt32(b["id"]);
		if((idA != idB) || a == null || b == null || a["linking_index"] == null || b["linking_index"] == null)
		{
			//Debug.Log("Default 0");
			
			//Debug.Log("!=: " + (idA != idB));
			
			return 0;
		}
		else
		{
			//Debug.LogWarning("CHECKING " + idA + " - " + idB);
			int indexA = System.Convert.ToInt32(a["linking_index"]);
			int indexB = System.Convert.ToInt32(b["linking_index"]);
			if(indexA < indexB)
			{
				//Debug.LogWarning("Sort -1");
				return -1;
			}
			else if(indexA > indexB)
			{
				//Debug.LogWarning("Sort 1");
				return 1;
			}
			else
			{
				//Debug.Log("Special 0");
				//Debug.Log("a: " + idA + " - " + indexA);
				//Debug.Log("b: " + idB + " - " + indexB);
				return 0;
			}
		}
	}
}

public class DataTable
{
    public DataTable()
    {
        Columns = new List<string>();
        Rows = new List<DataRow>();
    }
    
    public List<string> Columns { get; set; }
    public List<DataRow> Rows { get; set; }
    
    public DataRow this[int row]
    {
        get
        {
            return Rows[row];
        }
    }
    
    public void AddRow(object[] values)
    {
        if (values.Length != Columns.Count)
        {
            throw new IndexOutOfRangeException("The number of values in the row must match the number of column");
        }
        
        var row = new DataRow();
        for (int i = 0; i < values.Length; i++)
        {
            row[Columns[i]] = values[i];
        }
        
        Rows.Add(row);
    }
}

