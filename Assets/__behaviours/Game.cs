using UnityEngine;
using System.Collections;

// This class is static instead of a Singleton. This is because it is only used for other classes to reference and making it static will make
// the code shorter and more readable every place it is referenced
public static class Game 
{
    public enum Session
    {
        Premade,
        Custom,
        Single
    }

    private static Session m_session;
    public static Session session
    {
        get
        {
            return m_session;
        }
    }

    public static void SetSession(Session newSession)
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

    public static void SetData(Data newData)
    {
        m_data = newData;
    }
}
