using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class Analytics : Singleton<Analytics> 
{
    string m_url = "";
    string m_modelName = "activities";
    string m_dateTimeString = "yy-MM-dd HH:mm:ss";

    public string GetModelName()
    {
        return m_modelName;
    }

    public List<Activity> m_cachedActivities = new List<Activity>();

    Activity m_currentActivity;

    public class Activity
    {
        string m_email;
        string m_childName;
        string m_activityType;
        string m_activityName;
        int m_numCorrect;
        int m_numIncorrect;
        string m_dataType;
        List<int> m_incorrectData = new List<int>();
        string m_timeStarted;
        string m_timeEnded;

        public Activity(string myDataType)
        {
            m_dataType = myDataType;

            m_email = UserInfo.Instance.GetEmail();
            m_childName = UserInfo.Instance.GetCurrentUser();
            m_activityType = GameManager.activityType;
            m_activityName = GameManager.Instance.currentGameName;
            m_timeStarted = DateTime.Now.ToString(Analytics.Instance.m_dateTimeString);
        }

        public Activity() {}

        public void WriteData(BinaryWriter bw)
        {
            bw.Write(m_email);
            bw.Write(m_childName);
            bw.Write(m_activityType);
            bw.Write(m_activityName);
            bw.Write(m_numCorrect);
            bw.Write(m_numIncorrect);
            bw.Write(m_dataType);
            bw.Write(StringHelpers.CollectionToString<int>(m_incorrectData));
            bw.Write(m_timeStarted);
            bw.Write(m_timeEnded);
        }

        public void ReadData(BinaryReader br)
        {
            m_email = br.ReadString();
            m_childName = br.ReadString();
            m_activityType = br.ReadString();
            m_activityName = br.ReadString();
            m_numCorrect = br.ReadInt32();
            m_numIncorrect = br.ReadInt32();
            m_dataType = br.ReadString();

            string incorrectDataString = br.ReadString();

            try
            {
                m_incorrectData = StringHelpers.StringToList<int>(incorrectDataString);
            }
            catch
            {
                D.LogError("Could not convert incorrectDataString into List<int>");
            }

            m_timeStarted = br.ReadString();
            m_timeEnded = br.ReadString();
        }

        public void RecordTimeEnded()
        {
            m_timeEnded = DateTime.Now.ToString(Analytics.Instance.m_dateTimeString);
        }

        public void OnCorrect()
        {
            ++m_numCorrect;
        }

        public void OnIncorrect(int id)
        {
            ++m_numIncorrect;
            m_incorrectData.Add(id);
        }

        public void FillInForm(WWWForm form)
        {
            string modelName = Analytics.Instance.GetModelName();

            form.AddField (modelName + "[email]", m_email);
            form.AddField (modelName + "[child_name]", m_childName);
            form.AddField (modelName + "[activity_type]", m_activityType);
            form.AddField (modelName + "[activity_name]", m_activityName);
            form.AddField (modelName + "[num_correct]", m_numCorrect);
            form.AddField (modelName + "[num_incorrect]", m_numIncorrect);
            form.AddField (modelName + "[data_type]", m_dataType);
            form.AddField (modelName + "[incorrect_data]", StringHelpers.CollectionToString<int>(m_incorrectData));
            form.AddField (modelName + "[time_started]", m_timeStarted);
            form.AddField (modelName + "[time_ended]", m_timeEnded);
        }
    }

    public static void OnCorrect()
    {
        if (Instance.m_currentActivity != null)
        {
            Instance.m_currentActivity.OnCorrect();
        }
    }

    public static void OnIncorrect(int id)
    {
        if (Instance.m_currentActivity != null)
        {
            Instance.m_currentActivity.OnIncorrect(id);
        }
    }

    void Start()
    {
        GameManager.Instance.Cancelling += OnGameCancel;
    }
	
    public void CreateNewActivity(string dataType)
    {
        GameManager.Instance.CompletedSingle += OnGameComplete;
        m_currentActivity = new Activity(dataType);
    }

    void OnGameCancel()
    {
        GameManager.Instance.CompletedSingle -= OnGameComplete;
    }

    void OnGameComplete()
    {
        GameManager.Instance.CompletedSingle -= OnGameComplete;

        if (m_currentActivity != null)
        {
            // Save activity in temporary variable because GameManager might start next game and create new m_currentActivity before www has finished posting, this would prevent us from caching the activity if the post fails
            Activity activity = m_currentActivity;
            m_currentActivity = null;

            activity.RecordTimeEnded();

            WWWForm form = new WWWForm();
            activity.FillInForm(form);

            WWW www = new WWW(m_url, form);

            StartCoroutine(PostData(www, activity));
        }
    }

    IEnumerator PostData(WWW www, Activity activity)
    {
        yield return www;
        
        // check for errors
        if (www.error != null)
        {
            m_cachedActivities.Add(activity);
            Save();
        }    
    }
    
    void Save()
    {
        DataSaver ds = new DataSaver("ActivityCache");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);
        
        bw.Write(m_cachedActivities.Count);
        foreach (Activity activity in m_cachedActivities)
        {
            activity.WriteData(bw);
        }
        
        ds.Save(newData);
        
        bw.Close();
        newData.Close();
    }

    void Load()
    {
        DataSaver ds = new DataSaver("ActivityCache");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);
        
        if (data.Length != 0)
        {
            int numCachedActivities = br.ReadInt32();
            for(int i = 0; i < numCachedActivities; ++i)
            {
                Activity activity = new Activity();
                activity.ReadData(br);
            }
        }
        
        br.Close();
        data.Close();
    }
}
