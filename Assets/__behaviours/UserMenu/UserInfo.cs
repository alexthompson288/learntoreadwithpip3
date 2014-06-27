using UnityEngine;
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

        if (m_attemptLogin)
        {
            Debug.Log("ATTEMPTING LOGIN");

            DateTime expirationDate;
            try
            {
                expirationDate = StringHelpers.GetDateTimeYMD(m_expirationDate.Replace("\"", ""), "-");
            } catch
            {
                expirationDate = DateTime.Now.AddDays(-2);
            }

            // TODO: This is far too simple a test to determine if user is legal. 
            // For certain types of web exceptions (eg. no internet) then user is legal
            // However, for user exceptions (especially Expired) then user is illegal
            bool isUserLegal = false;
            Exception ex = null;
            try
            {
                isUserLegal = UserHelpers.IsUserLegal();
            } 
            catch (UserException userEx)
            {
                ex = userEx;
            } 
            catch (WebException webEx)
            {
                ex = webEx;
            }

            if (String.IsNullOrEmpty(m_email) || DateTime.Compare(expirationDate, DateTime.Now) < 0 || !isUserLegal)
            {
                GameObject.Instantiate(m_loginPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }

    public void SaveUserDetails(string myEmail, string myAccessToken, string myExpirationDate)
    {
        m_email = myEmail;
        m_accessToken = myAccessToken;
        m_expirationDate = myExpirationDate;
        Save();
    }

    public string GetEmail()
    {
        return m_email;
    }

	string m_currentUser = "";
	public string childName
	{
		get
		{
			return m_currentUser;
		}
	}

	Dictionary<string, string> m_users = new Dictionary<string, string>();

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

    string m_ipAddress = "";


#if UNITY_EDITOR
	[SerializeField]
	private bool m_overwrite;
#endif

    void PostData()
    {
        string url = "www.learntoreadwithpip.com/users";
        string modelName = "user";

        WWWForm form = new WWWForm();

        //Debug.Log("Posting user data: " + m_accountUsername);

        //form.AddField(modelName + "[account_username]", m_accountUsername);
        //form.AddField(modelName + "[email]", m_userEmail);
        form.AddField(modelName + "[user_type]", ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings())).m_userType);
        form.AddField(modelName + "[child_usernames]", CollectionHelpers.ConcatList(m_users.Keys.ToList()));
        form.AddField(modelName + "[platform]", Application.platform.ToString());
        form.AddField(modelName + "[ip_address]", m_ipAddress);

        WWW www = new WWW(url, form);

        //UserStats.Instance.WaitForRequest("User", www);
    }

    /*
    void FindIp()
    {
        string hostName = Dns.GetHostName();
        Debug.Log("HostName: " + hostName);
        IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
        foreach (IPAddress address in ipAddresses)
        {
            Debug.Log("ip: " + address.ToString());
        }
    }

    void FindIp2()
    {
        Debug.Log("ip2: " + Network.player.ipAddress);
    }
    */

    IEnumerator FindIpAddress()
    {
        m_waitForIpAddress = true;

        WWW myExtIPWWW = null;

        try
        {
            myExtIPWWW = new WWW("http://checkip.dyndns.org");
        }
        catch
        {
            Debug.Log("FindIpAddress - catch - after finding");
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
                    
                Debug.Log("ip address: " + myExtIP);

                m_ipAddress = myExtIP;
            }
            catch
            {
                Debug.Log("FindIpAddress - catch - after parsing address");
                m_ipAddress = "";
            }
        }

        m_waitForIpAddress = false;
    }

    bool m_waitForIpAddress = false;

    IEnumerator Start()
    {
        while (m_waitForIpAddress)
        {
            yield return null;
        }

        PostData();
    }
   

	public string GetCurrentUser ()
	{
		if(m_currentUser == null)
		{
			m_currentUser = "Pip";
		}

		return m_currentUser;
	}

	public Dictionary<string, string> GetUsers()
	{
		return m_users;
	}

	public void SetCurrentUser (string user) 
	{
        if (user != m_currentUser)
        {
            m_currentUser = user;
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

        //Debug.Log("UserInfo.Load()");
        //Debug.Log("data.Length: " + data.Length);
		
		if (data.Length != 0)
		{
            m_email = br.ReadString();
            m_accessToken = br.ReadString();
            m_expirationDate = br.ReadString();

			int numUsers = br.ReadInt32();
			for(int i = 0; i < numUsers; ++i)
			{
				string user = br.ReadString();
				string imageName = br.ReadString();
				m_users[user] = imageName;
			}

			m_currentUser = br.ReadString();
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
        bw.Write(m_accessToken);
        bw.Write(m_expirationDate);
		
		bw.Write(m_users.Count);
		foreach (KeyValuePair<string, string> kvp in m_users)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}

		bw.Write(m_currentUser);
		
		ds.Save(newData);
		
		bw.Close();
		newData.Close();
	}

    /*
    void Awake()
    {   
        //Debug.Log("UserInfo.Awake()");
        
        #if UNITY_STANDALONE || UNITY_ANDROID
        try
        {
            StartCoroutine(FindIpAddress());
            Debug.Log("Found ip address: " + m_ipAddress);
        }
        catch
        {
            m_waitForIpAddress = false;
            Debug.LogError("UserInfo.FindIpAddress - caller catch");
        }
        #endif
        
        #if UNITY_EDITOR
        if(m_overwrite)
        {
            Save ();
        }
        #endif
        
        Load();

        if (System.String.IsNullOrEmpty(m_accountUsername))
        {
            string dateTimeString = TimeHelpers.BuildDateTimeString(System.DateTime.Now);
            dateTimeString = dateTimeString.Replace("/", "_");
            dateTimeString = dateTimeString.Replace(":", "_");

            string rand = Random.Range(100000, 1000000).ToString();

            m_accountUsername = dateTimeString + rand;

            string newUser = "Pip";
            m_currentUser = newUser;
            CreateUser(newUser, "pip_state_a");
            
            Save();
        }
    }
    */
}
