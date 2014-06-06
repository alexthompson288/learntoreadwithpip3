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
        if (game != null)
        {
            m_label.text = game ["labeltext"] != null ? game ["labeltext"].ToString() : game ["name"].ToString();
            m_icon.spriteName = game ["name"].ToString() + "_complete";

            m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(game ["name"].ToString() + "_complete");
            if (m_temporaryIconTexture.mainTexture == null)
            {
                m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(game ["name"].ToString());
            }

            m_temporaryIconTexture.gameObject.SetActive(m_temporaryIconTexture.mainTexture != null);
        }
    }

    public string GetIconName()
    {
        return m_icon.spriteName;
    }

    public Texture2D GetTemporaryIconTexture()
    {
        return m_temporaryIconTexture.mainTexture as Texture2D;
    }
}
