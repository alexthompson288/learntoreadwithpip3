using System;
using System.Runtime.InteropServices;
using UnityEngine;
 
public class SqliteException : Exception
{
    public SqliteException(string message) : base(message)
    {
    
    }
}
 
public class SqliteDatabase
{
    const int SQLITE_OK = 0;
    const int SQLITE_ROW = 100;
    const int SQLITE_DONE = 101;
    const int SQLITE_INTEGER = 1;
    const int SQLITE_FLOAT = 2;
    const int SQLITE_TEXT = 3;
    const int SQLITE_BLOB = 4;
    const int SQLITE_NULL = 5;
        
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_open")]
    internal static extern int sqlite3_open(string filename, out IntPtr db);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_close")]
    internal static extern int sqlite3_close(IntPtr db);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_prepare_v2")]
    internal static extern int sqlite3_prepare_v2(IntPtr db, string zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_step")]
    internal static extern int sqlite3_step(IntPtr stmHandle);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_finalize")]
    internal static extern int sqlite3_finalize(IntPtr stmHandle);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_errmsg")]
    internal static extern IntPtr sqlite3_errmsg(IntPtr db);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_count")]
    internal static extern int sqlite3_column_count(IntPtr stmHandle);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_name")]
    internal static extern IntPtr sqlite3_column_name(IntPtr stmHandle, int iCol);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_type")]
    internal static extern int sqlite3_column_type(IntPtr stmHandle, int iCol);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_int")]
    internal static extern int sqlite3_column_int(IntPtr stmHandle, int iCol);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_text")]
    internal static extern IntPtr sqlite3_column_text(IntPtr stmHandle, int iCol);
 
    [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_double")]
    internal static extern double sqlite3_column_double(IntPtr stmHandle, int iCol);
 
    private IntPtr _connection;
    private bool IsConnectionOpen { get; set; }
 

    #region Public Methods
    
    public void Open(string path)
    {
        if (IsConnectionOpen)
        {
            throw new SqliteException("There is already an open connection");
        }
        
        if (sqlite3_open(path, out _connection) != SQLITE_OK)
        {
            throw new SqliteException("Could not open database file: " + path);
        }
        
        IsConnectionOpen = true;
    }
     
    public void Close()
    {
        if(IsConnectionOpen)
        {
            sqlite3_close(_connection);
        }
        
        IsConnectionOpen = false;
    }
 
    public void ExecuteNonQuery(string query)
    {
        if (!IsConnectionOpen)
        {
            throw new SqliteException("SQLite database is not open.");
        }

        IntPtr stmHandle = Prepare(query);
 
        if (sqlite3_step(stmHandle) != SQLITE_DONE)
        {
            throw new SqliteException("Could not execute SQL statement.");
        }
        
        Finalize(stmHandle);
    }

    public DataTable ExecuteQuery(string query)
    {
        if (!IsConnectionOpen)
        {
            throw new SqliteException("SQLite database is not open.");
        }
        
        IntPtr stmHandle = Prepare(query);
 
        int columnCount = sqlite3_column_count(stmHandle);
 
        var dataTable = new DataTable();
        for (int i = 0; i < columnCount; i++)
        {
            string columnName = Marshal.PtrToStringAnsi(sqlite3_column_name(stmHandle, i));
            dataTable.Columns.Add(columnName);
        }

        // Tom Mulvaney
        // Add a column called "tablename". It will contain the name of the table which contains the DataRow
        dataTable.Columns.Add("tablename");

        // TODO: Find an absoultely reliable way to parse the query to find the tablename. 
        //       sqlite documentation contains 2 promising functions: sqlite3_column_table_name and sqlite3_column_origin_name
        //       However, these APIs are only available if the library was compiled with the SQLITE_ENABLE_COLUMN_METADATA C-preprocessor symbol
        //       Additionally, these are taken from the C++ documentation which means that the Entry Point name might need to be demangled
        //       https://sqlite.org/c3ref/column_database_name.html

        // Tom Mulvaney
        // Find "tablename"
        string tableName = "";
        try
        {
            string fromString = " from ";
            int fromIndex = query.IndexOf(fromString);
            fromIndex += fromString.Length; 

            for(int i = fromIndex; i < query.Length; ++i)
            {
                if(!IsNullOrWhiteSpace(query[i].ToString()))
                {
                    tableName += query[i];
                }
                else
                {
                    break;
                }
            }
        }
        catch
        {
            //////D.LogError("Could not parse tablename");
            tableName = "default";
        }
        
        //populate datatable
        while (sqlite3_step(stmHandle) == SQLITE_ROW)
        {
            //object[] row = new object[columnCount];
            object[] row = new object[columnCount + 1]; // Tom Mulvaney: Add an extra column for tablename


            for (int i = 0; i < columnCount; i++)
            {
                switch (sqlite3_column_type(stmHandle, i))
                {
                    case SQLITE_INTEGER:
                        row[i] = sqlite3_column_int(stmHandle, i);
                        break;
                
                    case SQLITE_TEXT:
                        IntPtr text = sqlite3_column_text(stmHandle, i);
                        row[i] = Marshal.PtrToStringAnsi(text);
                        break;

                    case SQLITE_FLOAT:
                        row[i] = sqlite3_column_double(stmHandle, i);
                        break;
                    
                    case SQLITE_NULL:
                        row[i] = null;
                        break;
                }
            }

            // Tom Mulvaney
            // Set tablename
            row[columnCount] = tableName;

        
            dataTable.AddRow(row);
        }
        
        Finalize(stmHandle);
        
        return dataTable;
    }

    public bool IsNullOrWhiteSpace(string s)
    {
        return (String.IsNullOrEmpty(s) || s == " ");
    }
    
    public void ExecuteScript(string script)
    {
        string[] statements = script.Split(';');
        
        foreach (string statement in statements)
        {
            if (!string.IsNullOrEmpty(statement.Trim ()))
            {
                ExecuteNonQuery(statement);
            }
        }
    }
    
    #endregion
    
    #region Private Methods
 
    private IntPtr Prepare(string query)
    {
        IntPtr stmHandle;
        
        if (sqlite3_prepare_v2(_connection, query, query.Length, out stmHandle, IntPtr.Zero) != SQLITE_OK)
        {
            IntPtr errorMsg = sqlite3_errmsg(_connection);
            throw new SqliteException(Marshal.PtrToStringAnsi(errorMsg));
        }
        
        return stmHandle;
    }
 
    private void Finalize(IntPtr stmHandle)
    {
        if (sqlite3_finalize(stmHandle) != SQLITE_OK)
        {
            throw new SqliteException("Could not finalize SQL statement.");
        }
    }
    
    #endregion
}
