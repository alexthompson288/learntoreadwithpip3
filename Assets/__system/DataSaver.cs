using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class DataSaver 
{
    string m_name;    
    public DataSaver(string name)
    {
        m_name = name + ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).GetConvertedName();
    }

    public void Save(MemoryStream ms)
    {
        byte[] buffer = ms.GetBuffer();

        string base64 = Convert.ToBase64String(buffer);

        PlayerPrefs.SetString(m_name, base64);
    }

    public MemoryStream Load()
    {
        string base64 = PlayerPrefs.GetString(m_name);

        byte[] buffer = Convert.FromBase64String(base64);

        MemoryStream ms = new MemoryStream(buffer);

        return ms;
    }
}
