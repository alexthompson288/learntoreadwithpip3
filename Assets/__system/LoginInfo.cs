using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;

public class LoginInfo : Singleton<LoginInfo>
{
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
    
    bool m_hasExited = false;
    

    void OnApplicationQuit()
    {
        m_hasExited = true;
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
        if (!Debug.isDebugBuild)
        {
            m_attemptLogin = true;
        }
        
        if (m_attemptLogin)
        {
            AttemptLogin();
            StartCoroutine(CheckLogin());
        }
    }

    IEnumerator CheckLogin()
    {
        yield return new WaitForSeconds(10);
        if (m_hasExited)
        {
            AttemptLogin();
        }
        
        StartCoroutine(CheckLogin());
    }
    
    void AttemptLogin()
    {
        D.Log("ATTEMPTING LOGIN");
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
                    
                    D.Log("Got new token: " + m_expirationDate);
                }
            }
            catch(WebException ex)
            {
                if(ex.Response is System.Net.HttpWebResponse)
                {
                    D.Log((ex.Response as HttpWebResponse).StatusCode);
                }
            }
            catch
            {
                D.LogError("Could not get token using saved login details");
            }
        }
        
        DateTime expirationDate;
        try
        {
            expirationDate = DateTime.ParseExact(m_expirationDate, "yyyy-MM-dd", null);
        } 
        catch
        {
            D.Log("CATCHING EXPIRATION DATE");
            expirationDate = DateTime.Now.AddDays(-2);
        }
        
        
        D.Log("CHECK USER");
        bool isUserLegal = false;
        try
        {
            isUserLegal = LoginHelpers.IsUserLegal();
            D.Log("NO_ERROR - isUserLegal: " + isUserLegal);
        } 
        catch (LoginException ex)
        {
            D.Log("USER_EXCEPTION");
            LoginCoordinator.SetInfoText(ex);
        } 
        catch (WebException ex)
        {
            D.Log("WEB_EXCEPTION");
            if(ex.Response is System.Net.HttpWebResponse)
            {
                LoginCoordinator.SetInfoText(ex, false);
            }
            else
            {
                isUserLegal = true;
            }
        }
        
        if (String.IsNullOrEmpty(m_email) || DateTime.Compare(expirationDate, DateTime.Now) < 0 || !isUserLegal)
        {
            if (Application.loadedLevelName != m_loginSceneName)
            {
                TransitionScreen.Instance.ChangeLevel(m_loginSceneName, false);
            }
        }
        else if (Application.loadedLevelName == m_loginSceneName)
        {
            TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
        }
    }
    
    public void SaveUserDetails(string myEmail, string myPassword, string myAccessToken, string myExpirationDate)
    {
        m_email = myEmail;
        m_password = myPassword;
        m_accessToken = myAccessToken;
        m_expirationDate = myExpirationDate;
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

    public void MakeExpirationYesterday()
    {
        DateTime expirationDate = DateTime.Now.AddDays(-1);
    }

    public void Overwrite()
    {
        m_email = "";
        m_password = "";
        m_accessToken = "";
        m_expirationDate = "";
        
        Save();
    }

    void Load()
    {
        DataSaver ds = new DataSaver("MyUserInfo");
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
        DataSaver ds = new DataSaver("MyUserInfo");
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
