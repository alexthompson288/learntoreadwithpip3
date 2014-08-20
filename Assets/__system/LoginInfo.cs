using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;

public class LoginInfo : Singleton<LoginInfo>
{
    ////////////////////////////////////////////////////////////////////////////////////
    // Debugging Methods
    public void MakeExpirationYesterday()
    {
        //D.Log("LoginInfo.MakeExpirationYesterday()");
        DateTime expirationDate = DateTime.Now.AddDays(-1);
        m_expirationDate = expirationDate.ToString(LoginHelpers.dateTimeFormat);
        //D.Log("NewExpirationDate: " + m_expirationDate);
        Save();
    }

    public void Overwrite()
    {
        //D.Log("LoginInfo.Overwrite()");
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
    
    bool m_hasExited = false;

    public bool GetAttemptLogin()
    {
        return m_attemptLogin;
    }

    void OnApplicationPause()
    {
        //D.Log("LoginInfo.OnApplicationPause()");
        m_hasExited = true;
    }

    public void Logout()
    {
        m_email = "";
        m_password = "";
        m_accessToken = "";
        m_expirationDate = "";
        Save();

        TransitionScreen.Instance.ChangeLevel("NewLogin", false);
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
            InvokeRepeating("CheckForLogin", 10, 10);
        }
    }

    void CheckForLogin()
    {
        //D.Log("LoginInfo.CheckForLogin(): " + m_hasExited);
        if (m_hasExited)
        {
            AttemptLogin();
        } 
    }
    
    void AttemptLogin()
    {
        //D.Log("ATTEMPTING LOGIN");
        m_hasExited = false;
        
        if(!String.IsNullOrEmpty(m_email) && !String.IsNullOrEmpty(m_password))
        {
            //D.Log("TRYING TO UPDATE TOKEN");
            try
            {
                string tokenResponse = LoginHelpers.RequestToken(m_email, m_password);
                
                if(tokenResponse.Contains(LoginHelpers.accessPrefix) && tokenResponse.Contains(LoginHelpers.expirationPrefix))
                {
                    m_accessToken = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.accessPrefix, "\"");
                    m_expirationDate = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.expirationPrefix, "\"");
                    
                    //D.Log("Got new token: " + m_expirationDate);
                }
            }
            catch(WebException ex)
            {
                //D.LogError("Could not get token: WebException");
                if(ex.Response is System.Net.HttpWebResponse)
                {
                    //D.Log((ex.Response as HttpWebResponse).StatusCode);
                }
            }
            catch
            {
                //D.LogError("Could not get token using saved login details");
            }
        }

        //D.Log("CHECKING EXPIRATION DATE");
        DateTime expirationDate;
        try
        {
            expirationDate = DateTime.ParseExact(m_expirationDate, LoginHelpers.dateTimeFormat, null);
        } 
        catch
        {
            //D.Log("CATCHING EXPIRATION DATE");
            expirationDate = DateTime.Now.AddDays(-2);
        }
        
        
        bool isUserLegal = false;

        // Only check user if we actually have a token 
        if (!System.String.IsNullOrEmpty(m_accessToken))
        {
            //D.Log("FOUND TOKEN - CHECK USER");

            try
            {
                isUserLegal = LoginHelpers.IsUserLegal(m_accessToken);
                //D.Log("NO_ERROR - isUserLegal: " + isUserLegal);
            } 
            catch (LoginException ex)
            {
                //D.Log("USER_EXCEPTION: " + ex.exceptionType);
                LoginCoordinator.SetInfoText(ex);
            } 
            catch (WebException ex)
            {
                if (ex.Response is System.Net.HttpWebResponse)
                {
                    //D.Log("HTTP EXCEPTION: " + (ex.Response as System.Net.HttpWebResponse).StatusCode);
                    LoginCoordinator.SetInfoText(ex, false);
                } 
                else
                {
                    // If they have no internet connection then the user is legal (N.B. We still check for token validity below)
                    //D.Log("LEGAL WEB EXCEPTION");
                    isUserLegal = true;
                }
            }
        }
        else
        {
            //D.Log("NO TOKEN - CANNOT CHECK USER");
        }
        
        if (String.IsNullOrEmpty(m_email) || DateTime.Compare(expirationDate, DateTime.Now) < 0 || !isUserLegal)
        {
            //D.Log("FAIL - MUST LOGIN");
            //D.Log("m_email: " + m_email + " - " + !String.IsNullOrEmpty(m_email));
            //D.Log("m_expirationDate: " + m_expirationDate + " - " + (DateTime.Compare(expirationDate, DateTime.Now) >= 0));
            //D.Log("isUserLegal: " + isUserLegal);

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
