using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;

public class LoginInfo : Singleton<LoginInfo>
{
    [SerializeField]
    private GameObject m_loginPrefab;

    ////////////////////////////////////////////////////////////////////////////////////
    // Debugging Methods
    public void MakeExpirationYesterday()
    {
        //////D.Log("LoginInfo.MakeExpirationYesterday()");
        DateTime expirationDate = DateTime.Now.AddDays(-1);
        m_expirationDate = expirationDate.ToString(LoginHelpers.dateTimeFormat);
        //////D.Log("NewExpirationDate: " + m_expirationDate);
        Save();
    }

    public void Overwrite()
    {
        //////D.Log("LoginInfo.Overwrite()");
        m_email = "";
        m_password = "";
        m_accessToken = "";
        m_expirationDate = "";
        
        Save();
    }
    ////////////////////////////////////////////////////////////////////////////////////


    [SerializeField]
    private bool m_attemptLogin;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
#endif

    string m_email = "";
    string m_password = "";
    string m_accessToken = "";
    string m_expirationDate = "";
    
    string m_loginSceneName = "NewLogin";

    bool m_hasStarted = false;
    bool m_hasExited = false;

    bool m_isValid = false;

    public bool IsValid()
    {
        return m_isValid;
    }

    public bool GetAttemptLogin()
    {
        return m_attemptLogin;
    }

    void OnApplicationPause()
    {
        if (m_hasStarted)
        {
            m_hasExited = true;
        }
    }

    public void Logout()
    {
        m_isValid = false;

        m_email = "";
        m_password = "";
        m_accessToken = "";
        m_expirationDate = "";
        Save();
    }

    void Awake()
    {
#if UNITY_EDITOR
        if(m_overwrite)
        {
            Save();
        }
#endif

        Load();
    }

    void Start()
    {
        m_hasStarted = true;

        if (!Debug.isDebugBuild)
        {
            m_attemptLogin = true;
        }
        
        if (m_attemptLogin)
        {
            AttemptLogin();
            InvokeRepeating("CheckForLogin", 10, 10);
        }
    }

    void CheckForLogin()
    {
        //////D.Log("LoginInfo.CheckForLogin(): " + m_hasExited);
        if (m_hasExited)
        {
            AttemptLogin();
        } 
    }
    
    void AttemptLogin()
    {
        m_hasExited = false;
        
        if(!String.IsNullOrEmpty(m_email) && !String.IsNullOrEmpty(m_password))
        {
            try
            {
                string tokenResponse = LoginHelpers.RequestToken(m_email, m_password);
                
                if(tokenResponse.Contains(LoginHelpers.accessPrefix) && tokenResponse.Contains(LoginHelpers.expirationPrefix))
                {
                    m_accessToken = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.accessPrefix, "\"");
                    m_expirationDate = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.expirationPrefix, "\"");
                }
            }
            catch
            {
                D.LogError("Could not get new login token");
            }
        }

        DateTime expirationDate;
        try
        {
            expirationDate = DateTime.ParseExact(m_expirationDate, LoginHelpers.dateTimeFormat, null);
        } 
        catch
        {
            expirationDate = DateTime.Now.AddDays(-2);
        }

        m_isValid = String.IsNullOrEmpty(m_email) || String.IsNullOrEmpty(m_accessToken) || DateTime.Compare(expirationDate, DateTime.Now) < 0;
    }

    public void SpawnLogin()
    {
        Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_loginPrefab, NavMenu.Instance.transform);
    }
    
    public void SaveUserDetails(string myEmail, string myPassword, string myAccessToken, string myExpirationDate)
    {
        m_email = myEmail;
        m_password = myPassword;
        m_accessToken = myAccessToken;
        m_expirationDate = myExpirationDate;
        Debug.Log("Saving expirationData: " + m_expirationDate);
        Save();
    }
    
    public string GetEmail()
    {
        return m_email;
    }
    
    public string GetAccessToken()
    {
        return m_accessToken;
    }

    void Load()
    {
        DataSaver ds = new DataSaver("LoginInfo");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            m_email = br.ReadString();
            m_password = br.ReadString();
            m_accessToken = br.ReadString();
            m_expirationDate = br.ReadString();
        }
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver("LoginInfo");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_email);
        bw.Write(m_password);
        bw.Write(m_accessToken);
        bw.Write(m_expirationDate);
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }
}
