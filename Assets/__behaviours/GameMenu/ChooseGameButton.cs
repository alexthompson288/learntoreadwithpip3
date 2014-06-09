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

    DataRow m_game = null;

    void Awake()
    {
        System.Array.Sort(m_starSprites, CollectionHelpers.ComparePosX);
    }

	public void SetUp(DataRow game)
    {
        m_game = game;

        if (m_game != null)
        {
            m_label.text = m_game ["labeltext"] != null ? m_game ["labeltext"].ToString() : m_game ["name"].ToString();
            m_icon.spriteName = m_game ["name"].ToString() + "_complete";

            m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(m_game ["name"].ToString() + "_complete");
            if (m_temporaryIconTexture.mainTexture == null)
            {
                m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(m_game ["name"].ToString());
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

    public void Refresh(string colorName)
    {
        ScoreInfo.RefreshStarSprites(m_starSprites, m_game ["name"].ToString(), colorName);
        /*
        string gameName = m_game ["name"].ToString();
        string colorName = ColorInfo.GetColorString(pipColor);

        int numStars = 0;
        int targetScore = ScoreInfo.Instance.GetTargetScore(gameName, colorName);

        if (targetScore > 0)
        {
            float time = ScoreInfo.Instance.GetTime(gameName, colorName);

            if(time < ScoreInfo.Instance.GetThreeStar(gameName, colorName))
            {
                numStars = 3;
            }
            else if(time < ScoreInfo.Instance.GetTwoStar(gameName, colorName))
            {
                numStars = 2;
            }
            else
            {
                numStars = 1;
            }
        }

        for (int i = 0; i < m_starSprites.Length; ++i)
        {
            string spriteName = i < numStars ? m_starOnName : m_starOffName;
            m_starSprites[i].spriteName = spriteName;
        }
        */
    }
}
