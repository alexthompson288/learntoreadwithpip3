using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FlurryPip : Singleton<FlurryPip> 
{
    bool m_hasStarted = false;
    bool m_hasExited = false;

    string m_devApiKey = "JB23T9C942S2B8P8R742";
    string m_apiKey = "C6CHVND79YBYW3ZBRV54";

    // N.B. Probably skipped GeneralTimed3, maybe skipped GeneralTimed5 and 8
    string m_generalTimedEventName = "GeneralTimed9";

    string m_emailAttribute = "email";
    string m_ipAttribute = "ipAddress";

    Dictionary<string, string> m_generalTimedParameters = new Dictionary<string, string>();
    Dictionary<string, string> m_generalSavedParameters = new Dictionary<string, string>();

    float m_startTime;

    void OnApplicationPause()
    {
        if (m_hasStarted)
        {
            //D.Log("FlurryPip.OnApplicationPause() - HAS STARTED");

            m_hasExited = true;
            OnAppClose();
        }
    }

    void OnApplicationQuit()
    {
        //D.Log("FlurryPip.OnApplicationQuit()");
        OnAppClose();
    }

    void Start()
    {
        //D.Log("FlurryPip.Start()");
        Save();

        m_hasStarted = true;
        string apiKey = Debug.isDebugBuild ? m_devApiKey : m_apiKey;
                    
        FlurryAnalytics.startSession(apiKey);
        InvokeRepeating("CheckForExit", 10, 10);

        Load();
        
        StartCoroutine(OnAppOpen());
    }
    
    void CheckForExit()
    {
        if (m_hasExited)
        {
            m_hasExited = false;
            //D.Log("FlurryPip.CheckForExit() - HAS EXITED");
            StartCoroutine(OnAppOpen());
        } 
    }

    void LogSaved()
    {
        //D.Log("FlurryPip.LogSaved()");
        foreach (KeyValuePair<string, string> kvp in m_generalSavedParameters)
        {
            //D.Log(kvp.Key + ": " + kvp.Value);
        }
    }

    void LogTimed()
    {
        //D.Log("FlurryPip.LogTimed()");
        foreach (KeyValuePair<string, string> kvp in m_generalTimedParameters)
        {
            //D.Log(kvp.Key + ": " + kvp.Value);
        }
    }

    IEnumerator OnAppOpen()
    {
        Debug.Log("FlurryPip.OnAppOpen()");
        if (m_generalSavedParameters.Count > 0)
        {
            Debug.Log("Logging saved parameters");
            LogSaved();
            // N.B. Probably skipped GeneralSaved3, maybe skipped GeneralSaved5 and 8
            m_generalSavedParameters["time"] = (Time.time - m_startTime).ToString();
            FlurryAnalytics.logEventWithParameters("GeneralSaved9", m_generalSavedParameters, false);
            m_generalSavedParameters.Clear();
            Save();
        }

        m_startTime = Time.time;

        FindEmail();
        yield return StartCoroutine("FindIpAddress");
        Debug.Log("Logging timed parameters");
        LogTimed();
        FlurryAnalytics.logEventWithParameters(m_generalTimedEventName, m_generalTimedParameters, true);
    }

    void OnAppClose()
    {
        Debug.Log("FlurryPip.OnAppClose");
        StopAllCoroutines();
        FindEmail();
        Debug.Log("Ending timed parameters");
        LogTimed();
        FlurryAnalytics.endTimedEvent(m_generalTimedEventName, m_generalTimedParameters);
    }

    void FindEmail()
    {
        string email = LoginInfo.Instance.GetEmail();
        m_generalTimedParameters [m_emailAttribute] = email;
        m_generalSavedParameters [m_emailAttribute] = email;

        Save();
    }

    IEnumerator FindIpAddress()
    {
        m_generalTimedParameters [m_ipAttribute] = "";
        m_generalSavedParameters [m_ipAttribute] = "";

        WWW myExtIPWWW = null;
        
        try
        {
            myExtIPWWW = new WWW("http://checkip.dyndns.org");
        }
        catch
        {
            myExtIPWWW = null;
        }
        
        if (myExtIPWWW != null)
        {
            while (!myExtIPWWW.isDone)
            {
                yield return null;
            }
            
            try
            {
                string myExtIP = myExtIPWWW.data;
                myExtIP = myExtIP.Substring(myExtIP.IndexOf(":") + 1);
                myExtIP = myExtIP.Substring(0, myExtIP.IndexOf("<"));

                m_generalTimedParameters [m_ipAttribute] = myExtIP;
                m_generalSavedParameters [m_ipAttribute] = myExtIP;
            }
            catch
            {
                m_generalTimedParameters [m_ipAttribute] = "";
                m_generalSavedParameters [m_ipAttribute] = "";
            }
        } 

        Save();
    }

    void Load()
    {
        DataSaver ds = new DataSaver("FlurryPip");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            // Read here
            int numSavedParameters = br.ReadInt32();
            for(int i = 0; i < numSavedParameters; ++i)
            {
                m_generalSavedParameters[br.ReadString()] = br.ReadString();
            }
        }
        
        br.Close();
        data.Close();
    }
    
    void Save(string userName = null)
    {
        if (System.String.IsNullOrEmpty(userName))
        {
            userName = UserInfo.Instance.GetCurrentUserName();
        }
        
        DataSaver ds = new DataSaver("FlurryPip");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        // Write here
        bw.Write(m_generalSavedParameters.Count);
        foreach (KeyValuePair<string, string> kvp in m_generalSavedParameters)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
