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
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private bool m_starsAreTimeBased = true;

    DataRow m_game = null;

    string m_startSpriteName;

    public UIAtlas GetSpriteAtlas()
    {
        return m_icon.atlas;
    }

    public string GetStartSpriteName()
    {
        return m_startSpriteName;
    }

    void Awake()
    {
        m_startSpriteName = m_icon.spriteName;
        System.Array.Sort(m_starSprites, CollectionHelpers.CompareLocalPosX);
    }

	public void SetUp(DataRow game)
    {
        m_game = game;

        if (m_game != null)
        {
            m_label.text = m_game ["labeltext"] != null ? m_game ["labeltext"].ToString() : m_game ["name"].ToString();

            /*
            m_icon.spriteName = m_game ["name"].ToString() + "_complete";

            m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(m_game ["name"].ToString() + "_complete");
            if (m_temporaryIconTexture.mainTexture == null)
            {
                m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(m_game ["name"].ToString());
            }

            m_temporaryIconTexture.gameObject.SetActive(m_temporaryIconTexture.mainTexture != null);
            */
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

    public void Refresh(string colorName)
    {
        if (m_starsAreTimeBased)
        {
            ScoreInfo.RefreshTimeStars(m_starSprites, m_game ["name"].ToString(), colorName);
        }
        else
        {
            ScoreInfo.RefreshScoreStars(m_starSprites, m_game ["name"].ToString(), colorName);
        }
    }
}
