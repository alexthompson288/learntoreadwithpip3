﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System;

public class UserInfo : Singleton<UserInfo> 
{
    [SerializeField]
    private bool m_attemptLogin;
    [SerializeField]
    private GameObject m_loginPrefab;

    public delegate void UserChangeEventHandler();
    public event UserChangeEventHandler ChangingUser;

    string m_email = "";
    string m_password = "";
    string m_accessToken = "";
    string m_expirationDate = "";

    public string GetAccessToken()
    {
        return m_accessToken;
    }

    void Awake()
    {   
        #if UNITY_EDITOR
        if(m_overwrite)
        {
            Save ();
        }
        #endif
        
        Load();

        if (m_users.Count == 0)
        {
            CreateUser("Pip", "pip_state_a");
            m_currentUser = m_users.First();
            Save();
        }

        if (!Debug.isDebugBuild)
        {
            m_attemptLogin = true;
        }

        if (m_attemptLogin)
        {
            D.Log("ATTEMPTING LOGIN");

            if(!String.IsNullOrEmpty(m_email) && !String.IsNullOrEmpty(m_password))
            {
                try
                {
                    string tokenResponse = UserHelpers.RequestToken(m_email, m_password);

                    if(tokenResponse.Contains(UserHelpers.accessPrefix) && tokenResponse.Contains(UserHelpers.expirationPrefix))
                    {
                        m_accessToken = UserHelpers.ParseResponse(tokenResponse, UserHelpers.accessPrefix, "\"");
                        m_expirationDate = UserHelpers.ParseResponse(tokenResponse, UserHelpers.expirationPrefix, "\"");

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
                isUserLegal = UserHelpers.IsUserLegal();
                D.Log("NO_ERROR - isUserLegal: " + isUserLegal);
            } 
            catch (UserException ex)
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
                D.Log("INSTANTIATE LOGIN");
                //GameObject.Instantiate(m_loginPrefab, Vector3.zero, Quaternion.identity);

                StartCoroutine(LoadLoginScene());
            }
        }
    }

    IEnumerator LoadLoginScene()
    {
        yield return StartCoroutine(TransitionScreen.WaitForInstance());

        string loginSceneName = "NewLogin";
         
        if (Application.loadedLevelName != loginSceneName)
        {
            TransitionScreen.Instance.ChangeLevel(loginSceneName, false);
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

	Dictionary<string, string> m_users = new Dictionary<string, string>();
    KeyValuePair<string, string> m_currentUser;

    public string[] GetUserNames()
    {
        string[] users = new string[m_users.Count];

        int i = 0;
        foreach (KeyValuePair<string, string> kvp in m_users)
        {
            users[i] = kvp.Key;
            ++i;
        }

        return users;
    }

#if UNITY_EDITOR
	[SerializeField]
	private bool m_overwrite;
#endif

    public string GetCurrentUserName ()
    {   
        return m_currentUser.Key;
    }
    
    public Dictionary<string, string> GetUsers()
    {
        return m_users;
    }

    KeyValuePair<string, string> GetUser(string userName)
    {
        return m_users.Where(x => x.Key == userName).FirstOrDefault();
    }
    
    public void SetCurrentUser (string userName) 
    {
        KeyValuePair<string, string> lastUser = m_currentUser;

        m_currentUser = GetUser(userName);

        // Defensive: This should never execute
        if(m_currentUser.Equals(default(KeyValuePair<string, string>)))
        {
            D.LogError("UserInfo.SetCurrentUser() - m_users does not have key " + userName);
            foreach(KeyValuePair<string, string> kvp in m_users)
            {
                m_currentUser = kvp;
                break;
            }
        }

        if (m_currentUser.Key != lastUser.Key)
        {
            Save();
            
            if(ChangingUser != null)
            {
                ChangingUser();
            }
        }
    }
    
    public void CreateUser(string user, string imageName)
    {
        Dictionary<string, string> eventParameters = new Dictionary<string, string>();
        eventParameters.Add("Name", user);
        eventParameters.Add("Image", imageName);
        
        #if UNITY_IPHONE
        //FlurryBinding.logEventWithParameters("New User", eventParameters, false);
        #endif
        
        m_users[user] = imageName;
        Save ();
    }
    
    public void DeleteUser(string user)
    {
        m_users.Remove(user);
        Save ();
    }
    
    public bool HasUser(string user)
    {
        return m_users.ContainsKey(user);
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
            
            int numUsers = br.ReadInt32();
            for(int i = 0; i < numUsers; ++i)
            {
                string user = br.ReadString();
                string imageName = br.ReadString();
                m_users[user] = imageName;
            }

            string currentUserName = br.ReadString();

            if(m_users.Count > 0)
            {

                if(!String.IsNullOrEmpty(currentUserName))
                {
                    m_currentUser = GetUser(currentUserName);

                    // Defensive: This should never execute
                    if(m_currentUser.Equals(default(KeyValuePair<string, string>)))
                    {
                        D.LogError("UserInfo.Load() - m_users does not have key " + currentUserName);
                        m_currentUser = m_users.First();
                    }
                }
                else
                {
                    m_currentUser = m_users.First();
                }
            }
        }
        br.Close();
        data.Close();
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver("MyUserInfo");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write (m_email);
        bw.Write (m_password);
        bw.Write (m_accessToken);
        bw.Write (m_expirationDate);
        
        bw.Write(m_users.Count);
        foreach (KeyValuePair<string, string> kvp in m_users)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }

        string currentUserName = !String.IsNullOrEmpty(m_currentUser.Key) ? m_currentUser.Key : "";
        bw.Write(currentUserName);
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }


    ////////////////////////////////////////////////////////////////////////////////////////
    // TODO: Move the below functionality to Analytics

}
