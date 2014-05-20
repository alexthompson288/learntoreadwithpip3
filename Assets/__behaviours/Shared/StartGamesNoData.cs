using UnityEngine;
using System.Collections;

public class StartGamesNoData : MonoBehaviour 
{
    [SerializeField]
    private string m_returnScene = "";
    [SerializeField]
    private string[] m_gameNames;

	void OnClick()
    {
        if (System.String.IsNullOrEmpty(m_returnScene))
        {
            m_returnScene = Application.loadedLevelName;
        }

        GameManager.Instance.SetReturnScene(m_returnScene);


        foreach (string game in m_gameNames)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE name='" + game + "'");

            if(dt.Rows.Count > 0)
            {
                GameManager.Instance.AddGame(dt.Rows[0]);
            }
        }

        GameManager.Instance.StartGames();
    }
}
