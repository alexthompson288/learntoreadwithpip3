using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System.Data;

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
		////////D.Log("Override Equals");
		DataRow other = obj as DataRow;

		if(other == null)
		{
			return false;
		}
		else
		{
			return (Convert.ToInt32(this["id"]) == Convert.ToInt32(other["id"]) && this["tablename"].ToString() == other["tablename"].ToString());
		}
	}

	public override int GetHashCode()
	{
        return (this ["tablename"].ToString() + this ["id"].ToString()).GetHashCode();
		//return Convert.ToInt32(this["id"]);
	}

    public string GetTableName()
    {
        return this ["tablename"].ToString();
    }

    public int GetId()
    {
        return GetInt("id");
    }

    public int GetInt(string attributeName)
    {
        try
        {
            return Convert.ToInt32(this[attributeName]);
        }
        catch
        {
            ////////D.LogError(this["tablename"].ToString() + " has no int attribute called " + attributeName);
            return -1;
        }
    }
}

public class SectionComparer : IComparer<DataRow>
{
	public int Compare(DataRow a, DataRow b)
	{
		//////////D.Log("Compare");
		int idA = System.Convert.ToInt32(a["id"]);
		int idB = System.Convert.ToInt32(b["id"]);
		if((idA != idB) || a == null || b == null || a["linking_index"] == null || b["linking_index"] == null)
		{
			//////////D.Log("Default 0");
			
			//////////D.Log("!=: " + (idA != idB));
			
			return 0;
		}
		else
		{
			//////////D.LogWarning("CHECKING " + idA + " - " + idB);
			int indexA = System.Convert.ToInt32(a["linking_index"]);
			int indexB = System.Convert.ToInt32(b["linking_index"]);
			if(indexA < indexB)
			{
				//////////D.LogWarning("Sort -1");
				return -1;
			}
			else if(indexA > indexB)
			{
				//////////D.LogWarning("Sort 1");
				return 1;
			}
			else
			{
				//////////D.Log("Special 0");
				//////////D.Log("a: " + idA + " - " + indexA);
				//////////D.Log("b: " + idB + " - " + indexB);
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

