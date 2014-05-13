using UnityEngine;
using System.Collections;

public class ChooseGameButton : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private UITexture m_temporaryIconTexture;

	public void SetUp(DataRow game)
    {
        m_label.text = game["labeltext"] != null ? game["labeltext"].ToString() : game["name"].ToString();
        m_icon.spriteName = game["name"].ToString() + "_complete";

        m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(game ["name"].ToString() + "_complete");
        if (m_temporaryIconTexture.mainTexture == null)
        {
            m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(game ["name"].ToString());
        }

        m_temporaryIconTexture.gameObject.SetActive(m_temporaryIconTexture.mainTexture != null);
    }
}
