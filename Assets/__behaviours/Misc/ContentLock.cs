using UnityEngine;
using System.Collections;

public class ContentLock : Singleton<ContentLock> 
{
    [SerializeField]
    Lock m_lockType;

    public Lock lockType
    {
        get
        {
            return m_lockType;
        }
    }
    
    public enum Lock
    {
        None,
        Login,
        Buy
    }

    public bool IsPlusGameUnlocked(int id)
    {
        if (m_lockType == Lock.Buy)
        {
            string freeReadingGame = ProgrammeInfo.GetPlusReadingGames()[0];
            string freeMathsGame = ProgrammeInfo.GetPlusMathsGames()[0];

            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE id=" + id);
            string gameName = dt.Rows.Count > 0 ? dt.Rows[0]["name"].ToString() : "";

            return gameName == freeReadingGame || gameName == freeMathsGame || BuyInfo.Instance.IsGamePurchased(id);
        } 
        else
        {
            return m_lockType == Lock.None || (m_lockType == Lock.Login && LoginInfo.Instance.IsLoggedIn());
        }
    }
}
