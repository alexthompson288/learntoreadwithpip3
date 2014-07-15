using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class UserInfo : Singleton<UserInfo> 
{
    public delegate void UserChangeEventHandler();
    public event UserChangeEventHandler ChangingUser;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_overwrite;
#endif

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
}
