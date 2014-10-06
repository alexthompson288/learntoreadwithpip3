using UnityEngine;
using System.Collections;

public class ProgressGameButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private UISprite m_button;
    [SerializeField]
    private UILabel m_label;

    string m_gameName;

    public void SetUp(string myGameName, Color col)
    {
        m_gameName = myGameName;
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE name='" + m_gameName + "'");
        DataRow game = dt.Rows.Count > 0 ? dt.Rows[0] : null;

        m_label.text = game != null ? game ["labeltext"].ToString() : m_gameName;

        m_button.color = col;
    }

    void OnClick()
    {
        BasicGameMenuCoordinator.Instance.OnClickProgressGame(this);
    }

    public string GetGameName()
    {
        return m_gameName;
    }
}
