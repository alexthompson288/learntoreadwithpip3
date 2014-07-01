using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChooseGameButton : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private Transform m_starParent;
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private string m_blackboardSpriteName;
    [SerializeField]
    private int m_numPlayers = 1;

    DataRow m_game = null;

    public int GetNumPlayers()
    {
        return m_numPlayers;
    }

    public string GetBlackboardSpriteName()
    {
        return m_blackboardSpriteName;
    }

    public UIAtlas GetSpriteAtlas()
    {
        return m_icon.atlas;
    }

    void Awake()
    {
        System.Array.Sort(m_starSprites, CollectionHelpers.LocalLeftToRight);
    }

	public void SetUp(DataRow game)
    {
        m_game = game;

        if (m_game != null)
        {
            m_label.text = m_game ["labeltext"] != null ? m_game ["labeltext"].ToString() : m_game ["name"].ToString();
        }
    }

    public string GetIconName()
    {
        return m_icon.spriteName;
    }

    public void Refresh(string colorName)
    {
        ScoreInfo.RefreshStars(m_starSprites, m_game ["name"].ToString(), colorName);
    }

    public void UnlockScoreStars(List<Spline> m_starSplines, GameObject starPrefab)
    {

    }
}
