using UnityEngine;
using System.Collections;

public static class Game 
{
    public enum Session
    {
        Premade,
        Custom,
        Individual
    }

    private static Session m_session;
    public static Session session
    {
        get
        {
            return m_session;
        }
    }

    public void SetSession(Session newSession)
    {
        m_session = newSession;
    }

    public enum Data
    {
        Phonemes,
        Words,
        Keywords,
        Stories
    }

    private static Data m_data;
    public static Data data
    {
        get
        {
            return m_data;
        }
    }

    public void SetData(Data newData)
    {
        m_data = newData;
    }
}
